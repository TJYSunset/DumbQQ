using System;
using System.Collections.Generic;
using System.Text;

namespace DumbQQ.Models.Abstract
{
    public interface IClientExclusive
    {
        DumbQQClient Client { get; set; }
    }
}