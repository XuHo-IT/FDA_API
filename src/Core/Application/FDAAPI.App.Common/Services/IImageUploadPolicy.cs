using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FDAAPI.App.Common.Services
{
    public interface IImageUploadPolicy
    {
        bool IsAllowed(string? userId, string fileName, long sizeInBytes);
    }
}






