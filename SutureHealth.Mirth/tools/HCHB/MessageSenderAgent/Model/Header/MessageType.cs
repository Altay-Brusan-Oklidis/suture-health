using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Header
{
    /// <summary>
    /// MSH.9 - Message Type
    /// </summary>
    public class MessageType
    {
        [Required]
        public MessageCodeType Code { get; set; }
        [Required]
        public TriggerEvent Trigger { get; set; }
        [Required]
        public MessageStructureType Sturcture { get; set; }
    }
}
