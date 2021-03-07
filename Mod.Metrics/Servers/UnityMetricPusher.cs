using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using Prometheus;
using UnityEngine;
using UnityEngine.Networking;


namespace ValheimMods.Mod.MetricsExporter.Servers
{
    public delegate UnityWebRequest UnityWebRequestProvider();

    /// <summary>
    /// A metric server that regularly pushes metrics to a Prometheus PushGateway.
    /// </summary>
    public sealed class UnityMetricPusher : IMetricServer
    {
        private readonly ManualLogSource _log;
        private readonly Uri _targetUrl;
        private readonly UnityWebRequestProvider _unityWebRequestProvider;
        private readonly MonoBehaviour _mono;
        private readonly CollectorRegistry _registry;
        private readonly bool _skipTlsVerify;

        private Coroutine _pusherCoroutine;
        private readonly TimeSpan _interval;
        private readonly WaitForSecondsRealtime _waitForInterval;

        public UnityMetricPusher(MonoBehaviour mono, ManualLogSource log, UnityMetricPusherOptions options)
        {
            if (string.IsNullOrEmpty(options.Endpoint))
                throw new ArgumentNullException(nameof(options.Endpoint));

            if (string.IsNullOrEmpty(options.Job))
                throw new ArgumentNullException(nameof(options.Job));

            if (options.IntervalMilliseconds <= 0)
                throw new ArgumentException("Interval must be greater than zero", nameof(options.IntervalMilliseconds));

            var sb = new StringBuilder(string.Format("{0}/job/{1}", options.Endpoint!.TrimEnd('/'), options.Job));
            if (!string.IsNullOrEmpty(options.Instance))
            {
                sb.AppendFormat("/instance/{0}", options.Instance);
            }

            if (options.AdditionalLabels != null)
            {
                foreach (var pair in options.AdditionalLabels)
                {
                    if (pair == null || string.IsNullOrEmpty(pair.Item1) || string.IsNullOrEmpty(pair.Item2))
                        throw new NotSupportedException($"Invalid {nameof(UnityMetricPusher)} additional label: ({pair?.Item1}):({pair?.Item2})");

                    sb.AppendFormat("/{0}/{1}", pair.Item1, pair.Item2);
                }
            }

            if (!Uri.TryCreate(sb.ToString(), UriKind.Absolute, out _targetUrl))
            {
                throw new ArgumentException("Endpoint must be a valid url", "endpoint");
            }

            _mono = mono ?? throw new ArgumentNullException(nameof(mono));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _unityWebRequestProvider = options.UnityWebRequestProvider ?? (() => new UnityWebRequest());
            _interval = TimeSpan.FromMilliseconds(options.IntervalMilliseconds);
            _waitForInterval = new WaitForSecondsRealtime(options.IntervalMilliseconds / 1000.0f);
            _onError = options.OnError;
            _registry = options.Registry ?? Prometheus.Metrics.DefaultRegistry;
            _skipTlsVerify = options.SkipTlsVerify;

            _log.LogInfo($"Created UnityMetricPusher (endpoint = {options.Endpoint}, job = {options.Job}, instance = {options.Instance}, interval = {options.IntervalMilliseconds}");
        }

        private readonly Action<string> _onError;

        private IEnumerator Pusher()
        {
            do
            {
                _log.LogDebug("Preparing metrics push...");
                var unityWebRequest = _unityWebRequestProvider();

                unityWebRequest.uri = _targetUrl;
                unityWebRequest.method = UnityWebRequest.kHttpVerbPOST;

                byte[] data = null;
                string error = null;
                using (var ms = new MemoryStream())
                {
                    try
                    {
                        _registry.CollectAndExportAsTextAsync(ms, default).Wait();
                        data = ms.ToArray();
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
                    {
                        _log.LogDebug($"Failed to read metrics data: {e}");
                        error = e.Message;
                    }
#pragma warning restore CA1031 // Do not catch general exception types
                }

                if (data != null)
                {
                    _log.LogDebug($"Pushing metrics size = {data.Length}");
                    unityWebRequest.SetRequestHeader("Content-Type", PrometheusConstants.ExporterContentTypeMinimal);
                    unityWebRequest.uploadHandler = new UploadHandlerRaw(data);
                    unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                    if (_skipTlsVerify)
                    {
                        unityWebRequest.certificateHandler = new CertHandler();
                    }

                    yield return unityWebRequest.SendWebRequest();
                }

                if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError || data == null)
                {
                    HandleFailedPush(data == null ? error : unityWebRequest.error);
                }

                yield return _waitForInterval;
            } while (_pusherCoroutine != null);
        }

        private void HandleFailedPush(string error)
        {
            if (_onError != null)
            {
                // Asynchronous because we don't trust the callee to be fast.
                Task.Run(() => _onError(error));
            }
            else
            {
                // If there is no error handler registered, we write to trace to at least hopefully get some attention to the problem.
                _log.LogError($"Error in UnityMetricPusher: {error}");
            }
        }

        public IMetricServer Start()
        {
            _log.LogDebug("Starting UnityMetricPusher");
            _pusherCoroutine = _mono.StartCoroutine(Pusher());
            return this;
        }

        public Task StopAsync()
        {
            _log.LogDebug("Stopping UnityMetricPusher");
            if (_pusherCoroutine != null)
            {
                _mono.StopCoroutine(_pusherCoroutine);
                _pusherCoroutine = null;
            }
            return Task.CompletedTask;
        }

        public void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Stop();
        }

        private class CertHandler : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }
    }
}
