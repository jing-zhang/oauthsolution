﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using Oauth.Models;

namespace Oauth.Wrapper
{
    public interface IEndpoint
    {
        Task<bool> CallDelete(Guid Id);
        Task<DesObject> CallGet(int page, int pageSize);
        Task<DesObject> CallPost();
        Task<DesObject> CallPut();
    }
    public class Endpoint : BaseEndpoint, IEndpoint
    {
        /// <summary>
        /// BaseUrl http://upapi.edevnet.intrafinity.com/
        /// </summary>

        public Endpoint(IOAuth2Wrapper oAuth2Wrapper, int retryMax)
            : base(oAuth2Wrapper, retryMax)
        {

        }
        /// <summary>
        /// Dismiss Alert
        /// Delete /api/Alerts/{AlertId}
        /// </summary>
        /// <param name="alertId"></param>
        /// <returns></returns>
        public async Task<bool> CallDelete(Guid alertId)
        {
            string requestUri = string.Format("/api/Alerts/{0}", alertId);

            return await DeleteMethod(requestUri);
        }
        /// <summary>
        /// GET /api/Alerts/emergency
        /// Get My Alerts
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<DesObject> CallGet(int page = 0, int pageSize = 0)
        {
            string requestUrl = "api/users";

            if (page > 0 && pageSize > 0)
            {
                requestUrl += string.Format("?page={0}&pageSize={1}", page, pageSize);
            }

            return await GetMethod<DesObject>(requestUrl);
        }
        /// <summary>
        /// GET /api/Alerts/emergency/{alertId}
        /// GET Specified Alert
        /// </summary>
        /// <param name="alertId"></param>
        /// <returns></returns>
        public async Task<DesObject> CallPost()
        {
            object payload = new {
                gid = new Guid(),
                name = "asdf"
            };
            return await PostMethod<DesObject>("/api/Users", payload);
        }

        public async Task<DesObject> CallPut()
        {
            object payload = new {
                gid = new Guid(),
                name = "asdf"
            };
            return await PutMethod<DesObject>("/api/Users", payload);
        }

        public async Task<DesObject> CallPostWithRedirect()
        {
            if(_client != null){
                _client.Dispose();

                _client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
                _client.BaseAddress = _oAuth2Wrapper.baseUri;
                _client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization",
                $"Bearer {_oAuth2Wrapper.accessToken}");
            }

            object payload = new {
                gid = new Guid(),
                name = "asdf"
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/users", content);

            if (response.IsSuccessStatusCode)
            {
                DesObject upUser = JsonConvert.DeserializeObject<DesObject>(
                    await response.Content.ReadAsStringAsync());
                _client.Dispose();
                return upUser;

            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
            {
                var location = response.Headers.Location;
                response = await _client.GetAsync(location);
                if (response.IsSuccessStatusCode)
                {
                    DesObject upUser = JsonConvert.DeserializeObject<DesObject>(
                           await response.Content.ReadAsStringAsync());

                    _client.Dispose();
                    return upUser;
                }

            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                if (SignToken())
                    return await PostMethod<DesObject>("api/users", payload, 2);
            }

            _client.Dispose();
            return null;
        }
    }
}
