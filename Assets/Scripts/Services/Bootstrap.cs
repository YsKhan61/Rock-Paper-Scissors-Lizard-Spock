using System;
using System.Collections.Generic;
using RPSLS.Miscellaneous;
using RPSLS.Services.Base;
using UnityEngine;

namespace RPSLS.Services
{
    public class Bootstrap
    {
        private static Bootstrap _bootstrapInstance;

        private static Bootstrap BootstrapInstance
            => _bootstrapInstance ??= new Bootstrap();

        private readonly Dictionary<Type, ServiceBase> _serviceMap = new Dictionary<Type, ServiceBase>();

        /// <summary>
        /// Registers a service to the service map using a service type.
        /// </summary>
        /// <typeparam name="T">the type of service</typeparam>
        /// <param name="service"></param>
        internal static void RegisterService<T>(T service) where T : ServiceBase
        {
            if (!BootstrapInstance._serviceMap.ContainsKey(typeof(T)))
            {
                BootstrapInstance._serviceMap.Add(typeof(T), service);
                Debug.Log($"Registered New Service: {service.GetType().Name}");
            }
            else if (BootstrapInstance._serviceMap[typeof(T)] == null)
            {
                BootstrapInstance._serviceMap[typeof(T)] = service;
                Debug.Log($"Registered Existing Service: {service.GetType().Name}");
            }
            else Debug.LogWarning($"Registering Service Again: {service.GetType().Name}".ToColoredString(Color.red));

            GC.Collect();
        }

        /// <summary>
        /// Gets a service from the service map using a service type.
        /// </summary>
        /// <typeparam name="T">the type of the service needed</typeparam>
        /// <returns> the service instance of the given type </returns>
        internal static T GetService<T>() where T : ServiceBase =>
            BootstrapInstance._serviceMap[typeof(T)] as T;
    }
}