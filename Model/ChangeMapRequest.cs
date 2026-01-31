using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Model
{

    public class ChangeMapRequest
    {
        public string MapName { get; set; }
        public ChangeMapRequest(string body)
        {
            ChangeMapRequest cmr = JsonSerializer.Deserialize<ChangeMapRequest>(body);
            MapName = cmr.MapName;
        }

        public ChangeMapRequest()
        {

        }
    }
}
