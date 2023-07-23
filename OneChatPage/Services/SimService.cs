using HKDB.Models;
using OneChatPage.Extensions;
using OneChatPage.ViewModels;

namespace OneChatPage.Services
{
    public class SimService
    {
        public async Task<List<SimData>> SimMethod(EmbViewModel input, IEnumerable<Embedding> embeddings)
        {
            var SimDatas = new List<SimData>();
            //var embeddings = T.ToList();

            //平行處理
            await Task.Run(() =>
            {
                foreach (var item in embeddings)
                {
                    var sim = input.MessageEmbedding.CosineSimilarity(VStr.StrToVec(item.EmbeddingVector));

                    if (sim > 0.8)
                    {
                        var SimData = new SimData()
                        {
                            MessageEmbedding = input.MessageEmbedding,
                            DbEmbedding = item.EmbeddingVector,
                            DbId = item.EmbeddingId,
                            Similarity = sim
                        };
                        SimDatas.Add(SimData);
                    }
                }
            });
            //foreach (var item in embeddings)
            //{
            //     var sim = input.MessageEmbedding.CosineSimilarity(VStr.StrToVec(item.EmbeddingVector));

            //    if (sim > 0.7)
            //    {
            //        var SimData = new SimData()
            //        {
            //            MessageEmbedding = input.MessageEmbedding,
            //            DbEmbedding = item.EmbeddingVector,
            //            DbId = item.EmbeddingId,
            //            Similarity = sim
            //        };
            //        SimDatas.Add(SimData);
            //    }
            //}

            return SimDatas;
        }
    }
}
