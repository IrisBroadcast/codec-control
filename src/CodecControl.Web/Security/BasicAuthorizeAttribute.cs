using System;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BasicAuthorizeAttribute : TypeFilterAttribute
    {
        public BasicAuthorizeAttribute() : base(typeof(BasicAuthorizeFilter))
        {
        }
    }
}