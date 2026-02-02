using MessageSenderAgent.Model.Comment;
using MessageSenderAgent.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageSenderAgent.Builder
{
    public class NTESegmentBuilder
    {
        CommentModel commentModel;
        public NTESegmentBuilder(string? setId, string? comment = null) 
        {
            commentModel = new()
            {
                SetId = setId,
                Comment = comment,
                SourceOfComment = SourceOfComment.HCHB
            };            
        }

        public CommentModel Build()
        {
            if (commentModel.Comment is null)
                commentModel.Comment = "ph:" + Utilities.GetRandomalphabeticString(5) + 
                                       ";db:" + Utilities.GetRandomalphabeticString(5) + 
                                       ";tx:" + Utilities.GetRandomalphabeticString(5);
            return commentModel;
        }


    }
}
