﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Configuration;
using Oauth.Models;
using Newtonsoft.Json;

namespace Oauth.Wrapper
{
    public interface IOAuth2Wrapper
    {
        bool isInitialized { get; set; }
        string accessToken { get; set; }
        DateTime tokenExpires { get; set; }
        Uri baseUri { get; set; }
        Task<bool> GetBearTokenAsync();
        bool isAnonymous { get; set; }
    }

    public class OAuth2Wrapper : IOAuth2Wrapper
    {
        public bool isInitialized { get; set; }
        public string accessToken { get; set; }
        public DateTime tokenExpires { get; set; }
        public Uri baseUri { get; set; }
        public bool isAnonymous { get; set; }

        private string _accountId, _secret, _token_type, _requestUrl, _cache_id;

        public OAuth2Wrapper(string baseUrl)
        {
            isInitialized = false;
            baseUri = new Uri(baseUrl);
            isAnonymous = true;
        }
        public OAuth2Wrapper(string baseUrl, string requestUrl, string accountId, string secret, string cacheId)
        {
            _requestUrl = requestUrl;
            _accountId = accountId;
            _secret = secret;
            _cache_id = cacheId;
            isInitialized = false;
            baseUri = new Uri(baseUrl);
            accessToken = "";
            tokenExpires = DateTime.UtcNow;
            isAnonymous = false;


            if (tokenExpires < System.DateTime.UtcNow || accessToken.Length == 0)
                isInitialized = GetBearTokenAsync().Result;
            else
                isInitialized = true;
        }

        public async Task<bool> GetBearTokenAsync()
        {
            var client = new HttpClient();
            client.BaseAddress = baseUri;

            object criteria;

            if (_accountId.Length == 0)
            {
                criteria = new
                {
                    SecretKey = _secret
                };
            }
            else
            {
                criteria = new
                {
                    AccountId = _accountId,
                    SecretKey = _secret
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, _requestUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(criteria), Encoding.UTF8, "application/json")
            };

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var authorizationToken = JsonConvert.DeserializeObject<AuthenticationResult>(
                    await response.Content.ReadAsStringAsync());

                tokenExpires = authorizationToken.Expires.ToUniversalTime();
                _token_type = authorizationToken.TokenType;
                accessToken = authorizationToken.AccessToken;

                //put in cache

                isInitialized = true;
            }
            else
            {
                isInitialized = false;
            }

            client.Dispose();
            return isInitialized;
        }

    }
}
