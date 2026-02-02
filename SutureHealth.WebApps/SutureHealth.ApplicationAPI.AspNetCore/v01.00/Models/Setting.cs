

using System.Collections.Generic;

namespace SutureHealth.Application.v0100.Models;

public class Setting
{
    public IEnumerable<SettingDetail> Member { get; set; }
    public IEnumerable<SettingDetail> Organization { get; set; }
    public IEnumerable<SettingDetail> Application { get; set; }
}