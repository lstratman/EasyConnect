using MarcusW.VncClient;
using MarcusW.VncClient.Protocol.SecurityTypes;
using MarcusW.VncClient.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyConnect.Protocols.Vnc
{
    public class VncAuthenticationHandler : IAuthenticationHandler
    {
        protected string _password;

        public VncAuthenticationHandler(string password)
        {
            _password = password;
        }

        async Task<TInput> IAuthenticationHandler.ProvideAuthenticationInputAsync<TInput>(RfbConnection connection, ISecurityType securityType, IAuthenticationInputRequest<TInput> request)
        {
            if (typeof(TInput) == typeof(PasswordAuthenticationInput))
            {
                return (TInput) Convert.ChangeType(new PasswordAuthenticationInput(_password), typeof(TInput));
            }

            throw new InvalidOperationException($"The authentication type, {typeof(TInput).Name}, is not supported by this client.");
        }
    }
}
