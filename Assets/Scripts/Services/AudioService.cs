using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RPSLS.Audio;
using RPSLS.Miscellaneous;
using RPSLS.Services.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RPSLS.Services
{
    public class AudioService : ServiceBase
    {
        [SerializeField] private List<AudioScriptable> audioReferences;

        private Dictionary<string, AudioScriptable> _audioAssetMap =
            new Dictionary<string, AudioScriptable>();

        private readonly Dictionary<string, AudioSource> _loadedAudioMap =
            new Dictionary<string, AudioSource>();

        private GameObject _audioParent;

        /// <summary>
        /// Play an audio using the audio key.
        /// </summary>
        /// <param name="key"></param>
        internal void PlayAudio(string key) =>
            StartCoroutine(PlayRoutine(key));

        /// <summary>
        /// Stop an audio using the audio key.
        /// </summary>
        /// <param name="key"></param>
        internal void StopAudio(string key)
        {
            if (_loadedAudioMap.ContainsKey(key))
                _loadedAudioMap[key].Stop();
            else Debug.LogError($"Key {key} was not present");
        }

        /// <summary>
        /// Stop all audio.
        /// </summary>
        internal void StopAllAudio()
        {
            foreach (var audioSource in _loadedAudioMap.Values)
                audioSource.Stop();
        }

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(SegregateAudioFiles());
        }

        protected override void RegisterService() =>
            Bootstrap.RegisterService(this);

        private IEnumerator SegregateAudioFiles()
        {
            yield return null;
            _audioAssetMap = audioReferences.ToDictionary(x => x.AudioTag, x => x);
            _audioParent = new GameObject("AudioParent");
            _audioParent.transform.SetParent(transform);
        }

        private IEnumerator PlayRoutine(string key)
        {
            if (_loadedAudioMap.ContainsKey(key))
            {
                Debug.Log("Audio Already Loaded".ToColoredString(Color.red));
                if (_audioAssetMap[key].AssetReference.OperationHandle.Status == AsyncOperationStatus.Succeeded &&
                    !_loadedAudioMap[key].isPlaying && !_audioAssetMap[key].isMusic)
                    _loadedAudioMap[key].Play();
                yield break;
            }

            var audioData = _audioAssetMap[key];
            var handle = audioData.AssetReference.LoadAssetAsync<AudioClip>();
            yield return handle;

            var loadedAudioClip = handle.Result;
            AudioScriptable.InstantiateAudio(_audioAssetMap[key], _audioParent, in loadedAudioClip, out var source);

            if (!source.isPlaying) source.Play();
            if (!_loadedAudioMap.ContainsKey(key))
                _loadedAudioMap.Add(key, source);
            else Debug.LogError($"Key {key} was already present");

            yield return new WaitWhile(() => source.isPlaying);
            Destroy(source);
            _loadedAudioMap.Remove(key);
            Addressables.Release(handle);
        }

        [ContextMenu("Generate Audio Tags")]
        private void GenerateAudioTags()
        {
            var sfxFiles = Directory.EnumerateFiles(Path.Combine("Assets", "Media", "Audio"), "*.mp3");
            var builder = new StringBuilder("namespace RPSLS.Audio\n{\n\tpublic static class AudioTags\n\t{\n");
            foreach (var sfxFile in sfxFiles)
            {
                var fName = Path.GetFileNameWithoutExtension(sfxFile);
                builder.AppendLine($"\t\tpublic const string {fName.ToSnakeCase().ToUpper()} = \"{fName}\";");
            }

            builder.AppendLine("\t}\n}");

            var scriptDir = Path.Combine("Assets", "Scripts", "Audio", "AudioTags.cs");
            File.WriteAllText(scriptDir, builder.ToString());
        }
    }
}