﻿// Licensed to the Apache Software Foundation(ASF) under one
// or more contributor license agreements.See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Transport;

namespace Jaeger.Thrift.Senders.Internal
{
    // ReSharper disable once InconsistentNaming
    public class THttpTransport : TEndpointTransport
    {
        private readonly Uri _uri;

        private int _connectTimeout = 30000; // Timeouts in milliseconds
        private HttpClient _httpClient;
        private Stream _inputStream;
        private MemoryStream _outputStream = new MemoryStream();
        private bool _isDisposed;

        public THttpTransport(Uri uri, IDictionary<string, string> customRequestHeaders,
            HttpClientHandler handler = null, IEnumerable<X509Certificate> certificates = null,
            string userAgent = null,
            IDictionary<string, object> customProperties = null)
            : base(null)
        {
            _uri = uri;

            if (!string.IsNullOrEmpty(userAgent))
            {
                UserAgent = userAgent;
            }

            CustomProperties = customProperties ?? new Dictionary<string, object>();

            // due to current bug with performance of Dispose in netcore https://github.com/dotnet/corefx/issues/8809
            // this can be switched to default way (create client->use->dispose per flush) later

            handler ??= new HttpClientHandler();
            certificates ??= Enumerable.Empty<X509Certificate>();
            _httpClient = CreateClient(handler, certificates, customRequestHeaders);
        }

        // According to RFC 2616 section 3.8, the "User-Agent" header may not carry a version number
        public string UserAgent { get; } = "Thrift netstd THttpClient";

        public override bool IsOpen => true;

        public HttpRequestHeaders RequestHeaders => _httpClient.DefaultRequestHeaders;

        public IDictionary<string, object> CustomProperties { get; }

        public MediaTypeHeaderValue ContentType { get; set; }

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancellationToken);
            }
        }

        public override void Close()
        {
            if (_inputStream != null)
            {
                _inputStream.Dispose();
                _inputStream = null;
            }

            if (_outputStream != null)
            {
                _outputStream.Dispose();
                _outputStream = null;
            }

            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }

        public override async ValueTask<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return await Task.FromCanceled<int>(cancellationToken);
            }

            if (_inputStream == null)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "No request has been sent");
            }

            CheckReadBytesAvailable(length);

            try
            {
                var ret = await _inputStream.ReadAsync(buffer, offset, length, cancellationToken);

                if (ret == -1)
                {
                    throw new TTransportException(TTransportException.ExceptionType.EndOfFile, "No more data available");
                }
                
                CountConsumedMessageBytes(ret);
                return ret;
            }
            catch (IOException iox)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, iox.ToString());
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                await Task.FromCanceled(cancellationToken);
            }

            await _outputStream.WriteAsync(buffer, offset, length, cancellationToken);
        }

        private HttpClient CreateClient(HttpClientHandler handler, IEnumerable<X509Certificate> certificates, IDictionary<string, string> customRequestHeaders)
        {
            if (certificates != null)
            {
                handler.ClientCertificates.AddRange(certificates.ToArray());
            }

            handler.AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip;

            var httpClient = new HttpClient(handler);

            if (_connectTimeout > 0)
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(_connectTimeout);
            }

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-thrift"));
            
            // Clear any user agent values to avoid drift with the field value
            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);

            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            if (customRequestHeaders != null)
            {
                foreach (var item in customRequestHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                }
            }

            return httpClient;
        }

        private HttpRequestMessage CreateRequestMessage(StreamContent streamContent)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _uri)
            {
                Content = streamContent
            };

            foreach (var requestProperty in CustomProperties)
            {
                requestMessage.Properties.Add(requestProperty);
            }

            return requestMessage;
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            try
            {
                _outputStream.Seek(0, SeekOrigin.Begin);

                using (var contentStream = new StreamContent(_outputStream))
                {
                    contentStream.Headers.ContentType = ContentType ?? new MediaTypeHeaderValue(@"application/x-thrift");

                    var requestMessage = CreateRequestMessage(contentStream);
                    var msg = await _httpClient.SendAsync(requestMessage, cancellationToken);
                    var response = msg.EnsureSuccessStatusCode();

                    _inputStream?.Dispose();
                    _inputStream = await response.Content.ReadAsStreamAsync();
                    if (_inputStream.CanSeek)
                    {
                        _inputStream.Seek(0, SeekOrigin.Begin);
                    }
                }
            }
            catch (IOException iox)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, iox.ToString());
            }
            catch (HttpRequestException wx)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown,
                    "Couldn't connect to server: " + wx);
            }
            catch (Exception ex)
            {
                throw new TTransportException(TTransportException.ExceptionType.Unknown, ex.Message);
            }
            finally
            {
                _outputStream = new MemoryStream();
                ResetConsumedMessageSize();
            }
        }

        // IDisposable
        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _inputStream?.Dispose();
                    _outputStream?.Dispose();
                    _httpClient?.Dispose();
                }
            }
            _isDisposed = true;
        }
    }
}