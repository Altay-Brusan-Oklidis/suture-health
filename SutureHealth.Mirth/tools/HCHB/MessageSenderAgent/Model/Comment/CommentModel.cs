using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Model.Comment
{
    public class CommentModel
    {
        public string? SetId { get; set; }
        [MaxLength(8)]
        public SourceOfComment SourceOfComment { get; set; }
        public string? Comment { get; set; }
    }
}
