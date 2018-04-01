﻿using System;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Oauth.Wrapper
{
    public class BaseEndpoint
    {
        internal IOAuth2Wrapper _oAuth2Wrapper;
        internal string _token;
        internal HttpClient _client;
        internal int _retry_max;

        public BaseEndpoint(IOAuth2Wrapper oAuth2Wrapper, int retryMax)
        {
            if (oAuth2Wrapper.isAnonymous)
            {
                _client = new HttpClient();
                _client.BaseAddress = oAuth2Wrapper.baseUri;
                _retry_max = retryMax;
                _oAuth2Wrapper = oAuth2Wrapper;
                return;
            }

            _oAuth2Wrapper = oAuth2Wrapper;
            _retry_max = retryMax;

            if (_oAuth2Wrapper.isInitialized && _oAuth2Wrapper.tokenExpires > DateTime.UtcNow)
                _token = _oAuth2Wrapper.accessToken;
            else
                _oAuth2Wrapper.isInitialized = _oAuth2Wrapper.GetBearTokenAsync().Result;

            if (_oAuth2Wrapper.isInitialized)
            {
                _client = new HttpClient();
                _client.BaseAddress = _oAuth2Wrapper.baseUri;
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization",
                $"Bearer {_oAuth2Wrapper.accessToken}");
            }
        }

        internal async Task<T> GetMethod<T>(string requestUrl, int retry = 1)
        {
            if (_client == null)
            {
                //Token not authrized
                return default(T);
            }

            var response = await _client.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                T results = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                _client.Dispose();
                return results;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                if (retry < _retry_max)
                {
                   if (SignToken())
                        return await GetMethod<T>(requestUrl, retry + 1);
                }
            }

            _client.Dispose();
            return default(T);
        }

        internal async Task<T> PostMethod<T>(string requestUrl, object payload, int retry = 1)
        {
            if (_client == null)
            {
                //Token not authrized
                return default(T);
            }

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(requestUrl, content);

            if (response.IsSuccessStatusCode)
            {
                T upUser = JsonConvert.DeserializeObject<T>(
                    await response.Content.ReadAsStringAsync());
                _client.Dispose();
                return upUser;

            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                if (retry < _retry_max)
                {
                    if (SignToken())
                        return await PostMethod<T>(requestUrl, payload, retry + 1); ;
                }
            }

            _client.Dispose();
            return default(T);
        }

        internal async Task<T> PutMethod<T>(string requestUrl, object payload, int retry = 1)
        {
            if (_client == null)
            {
                //Token not authrized
                return default(T);
            }

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync(requestUrl, content);

            if (response.IsSuccessStatusCode)
            {
                T upUser = JsonConvert.DeserializeObject<T>(
                    await response.Content.ReadAsStringAsync());
                _client.Dispose();
                return upUser;

            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                if (retry < _retry_max)
                {
                    if (SignToken())
                        return await PutMethod<T>(requestUrl, payload, retry + 1); ;
                }
            }

            _client.Dispose();
            return default(T);
        }

        internal async Task<bool> DeleteMethod(string requestUrl, int retry = 1)
        {
            if (_client == null)
            {
                //Token not authrized
                return false;
            }

            var response = await _client.DeleteAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                _client.Dispose();
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                if (retry < _retry_max)
                {
                    if (SignToken())
                        return await DeleteMethod(requestUrl, retry + 1); ;
                }
            }

            _client.Dispose();
            return false;
        }

        internal bool SignToken()
        {
            _oAuth2Wrapper.isInitialized = _oAuth2Wrapper.GetBearTokenAsync().Result;
            if (_oAuth2Wrapper.isInitialized)
            {
                _client.Dispose();
                
                _client = new HttpClient();
                _client.BaseAddress = _oAuth2Wrapper.baseUri;
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization",
                $"Bearer {_oAuth2Wrapper.accessToken}");
            }
            return _oAuth2Wrapper.isInitialized;
        }


        public async Task<bool> HealthPing()
        {
            string requestUri = "/api/Health/Ping";

            var response = await _client.GetAsync(requestUri);

            return response.IsSuccessStatusCode;
        }
    }
}
