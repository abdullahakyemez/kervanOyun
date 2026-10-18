using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kervan.Core
{
    /// <summary>
    /// Yeni static Singleton açmadan sahneler arası kalıcı servisleri (GameSession, AudioManager,
    /// SaveService, SceneLoader vb.) tek merkezden kaydetmek ve erişmek için kullanılan hafif servis bulucu.
    /// </summary>
    public static class Services
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Belirtilen türdeki servisi kayıt defterine ekler veya günceller.
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                Debug.LogError($"[Services] Null bir servis kaydedilemez: {typeof(T).Name}");
                return;
            }

            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Belirtilen türdeki servisi kayıt defterinden kaldırır.
        /// </summary>
        public static void Unregister<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object existing) && ReferenceEquals(existing, service))
            {
                _services.Remove(type);
            }
        }

        /// <summary>
        /// Kayıtlı servisi döndürür. Kayıtlı değilse hata loglar ve null döner.
        /// </summary>
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out object service))
            {
                return (T)service;
            }

            Debug.LogError($"[Services] İstenen servis kayıtlı değil: {typeof(T).Name}. Bootstrapper çalıştığından emin olun.");
            return null;
        }

        /// <summary>
        /// Servis kayıtlıysa güvenli biçimde döndürür; hata loglamaz.
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out object found))
            {
                service = (T)found;
                return service != null;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Belirtilen servis türünün kayıtlı olup olmadığını kontrol eder.
        /// </summary>
        public static bool IsRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Tüm servis kayıtlarını temizler (Domain Reload kapalı olduğunda temiz başlangıç için).
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
