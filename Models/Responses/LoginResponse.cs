using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Responses
{
    public class LoginResponse
    {
        public bool IsSuccessful { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public LocalUserInfo LocalUserInfo { get; set; }
        public static LoginResponse SuccessResponse(LocalUserInfo userInfo)
        {
            return new LoginResponse() { IsSuccessful = true, LocalUserInfo = userInfo };
        }
        public static LoginResponse ErrorResponse(IEnumerable<string> errors)
        {
            return new LoginResponse() { IsSuccessful = false, Errors = errors };
        }
        public static LoginResponse ErrorResponse(string error)
        {
            return new LoginResponse()
            {
                IsSuccessful = false,
                Errors = new List<string>() { error }
            };
        }
    }
}
