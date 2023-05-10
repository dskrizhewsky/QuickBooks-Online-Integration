#region Copyright Notice
/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2023 Dmytro Skryzhevskyi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using Intuit.Ipp.Core;
using Intuit.Ipp.Core.Configuration;
using Intuit.Ipp.DataService;
using Intuit.Ipp.OAuth2PlatformClient;
using Intuit.Ipp.Security;

namespace Dmytro.Skryzhevskyi.ExternalAccountingTools
{
    
    public class Authorization
    {
        private OAuth2Client _oauthClient;
        private TokenClient _tokenClient;
        private readonly AuthConfig _authConfig;

        protected Authorization()
        {
        }

        public Authorization(AuthConfig config)
        {
            _authConfig = config;
            SetTokenClient();
            SetAuthClient();
        }

        public ServiceContext CreateContext(Token token)
        {
            var oauthValidator = new OAuth2RequestValidator(token.AccessToken);
            var context = new ServiceContext(_authConfig.CompanyId, IntuitServicesType.QBO, oauthValidator);
            context.IppConfiguration.BaseUrl.Qbo =
                $"{_authConfig.GetBaseUrl()}";
            context.IppConfiguration.Message.Request.SerializationFormat = SerializationFormat.Json;
            context.IppConfiguration.Message.Response.SerializationFormat = SerializationFormat.Json;
            return context;
        }


        public DataService GetDataService(ServiceContext context)
        {
            return new DataService(context);
        }

        private void SetTokenClient()
        {
            var endPoint = _authConfig.GetTokenClientEndPoint();
            _tokenClient = new TokenClient(endPoint, _authConfig.ClientId, _authConfig.ClientSecret);
        }

        private void SetAuthClient()
        {
            _oauthClient = new OAuth2Client(_authConfig.ClientId, _authConfig.ClientSecret,
                _authConfig.GetRedirectUrl(), _authConfig.GetAppEnvironment());
        }


        public Token RequestToken(string authCode)
        {
            var accessTokenCallResponse =
                _tokenClient.RequestTokenFromCodeAsync(authCode, _authConfig.GetRedirectUrl()).Result;
            if (accessTokenCallResponse.IsError)
            {
                throw new AuthenticationException(
                    $"Token request error: {accessTokenCallResponse.Error} : {accessTokenCallResponse.ErrorDescription}");
            }

            var token = new Token(authCode, accessTokenCallResponse.AccessToken,
                accessTokenCallResponse.AccessTokenExpiresIn, accessTokenCallResponse.RefreshToken,
                accessTokenCallResponse.RefreshTokenExpiresIn);
            return token;
        }


        public Token RequestRefreshToken(string refreshToken)
        {
            var accessTokenCallResponse = _tokenClient.RequestRefreshTokenAsync(refreshToken).Result;
            if (accessTokenCallResponse.IsError)
            {
                throw new Exception($"{accessTokenCallResponse.Error} {accessTokenCallResponse.ErrorDescription}");
            }

            var token = new Token(accessTokenCallResponse.AccessToken, accessTokenCallResponse.AccessTokenExpiresIn,
                accessTokenCallResponse.RefreshToken, accessTokenCallResponse.RefreshTokenExpiresIn);
            return token;
        }

        /// <summary>
        /// Auth popup windows example
        /// Response._Redirect(GetWebAuthRequestUrl(), "_blank", "menubar=0,scrollbars=1,width=780,height=900,top=10");
        /// </summary>
        /// <returns>Authorization URL</returns>
        public string GetWebAuthRequestUrl()
        {
            var scopes = new List<OidcScopes> { OidcScopes.Accounting };
            var authorizationRequest = _oauthClient.GetAuthorizationURL(scopes);
            return authorizationRequest;
        }
    }

    public abstract class AuthConfig
    {
        public AuthConfig()
        {
        }

        public AuthConfig(string clientId, string clientSecret, string companyId)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            CompanyId = companyId;
            Timeout = 1000 * 60;
        }

        public int Timeout { get; protected set; }
        public string ClientId { get; protected set; }
        public string ClientSecret { get; protected set; }
        public string CompanyId { get; protected set; }

        public abstract string GetBaseUrl();

        public string GetTokenClientEndPoint()
        {
            return "https://oauth.platform.intuit.com/oauth2/v1/tokens/bearer";
        }

        public string GetRedirectUrl()
        {
            return "https://developer.intuit.com/v2/OAuth2Playground/RedirectUrl";
        }

        public abstract string GetAppEnvironment();
    }

    public class AuthConfigSandbox : AuthConfig
    {
        public AuthConfigSandbox()
        {
            ClientId = "";
            ClientSecret = "";
            CompanyId = "";
            ClientId = "";
            ClientSecret = "";
        }

        public AuthConfigSandbox(string clientId, string clientSecret, string companyId) : base(clientId, clientSecret,
            companyId)
        {
        }

        public override string GetBaseUrl()
        {
            return "https://sandbox-quickbooks.api.intuit.com/";
        }

        public override string GetAppEnvironment()
        {
            return "sandbox";
        }
    }

    public class AuthConfigProduction : AuthConfig
    {
        public AuthConfigProduction()
        {
            ClientId = "";
            ClientSecret = "";
            CompanyId = "";
        }

        public AuthConfigProduction(string clientId, string clientSecret, string companyId) : base(clientId,
            clientSecret, companyId)
        {
        }

        public override string GetBaseUrl()
        {
            return "https://quickbooks.api.intuit.com/";
        }

        public override string GetAppEnvironment()
        {
            return "production";
        }
    }


    public class Token
    {
        public Token(string authCode, string accToken, long accTokenExpiresIn, string refreshToken,
            long refreshTokenExpiresIn)
        {
            Updated = DateTime.Now;
            AuthorizationCode = authCode;
            AccessToken = accToken;
            AccessTokenExpiresIn = accTokenExpiresIn;
            RefreshToken = refreshToken;
            RefreshExpiresIn = refreshTokenExpiresIn;
        }

        public Token(string accToken, long accTokenExpiresIn, string refreshToken, long refreshTokenExpiresIn) : this(
            string.Empty, accToken, accTokenExpiresIn, refreshToken, refreshTokenExpiresIn)
        {
        }

        public DateTime Updated { get; private set; }
        public string AuthorizationCode { get; private set; }
        public string AccessToken { get; private set; }
        public long AccessTokenExpiresIn { get; private set; }
        public string RefreshToken { get; private set; }
        public long RefreshExpiresIn { get; private set; }

        public DateTime GetLastPossibleUpdateDate()
        {
            return Updated.AddSeconds(RefreshExpiresIn);
        }

        public bool RefreshExpired()
        {
            return DateTime.Now >= GetLastPossibleUpdateDate();
        }
    }
}