using HKDB.Data;
using HKDB.Models;
using OneChatPage.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using OneChatPage.Services;
using Microsoft.EntityFrameworkCore;
using OneChatPage.Extensions;
using System.Linq;

namespace OneChatPage.Controllers
{
    public class ChatController : Controller
    {
        private readonly HKContext _ctx;
        private readonly TranslateService _tr;
        private readonly SimService _sim;
        private readonly GptService _gpt;

        public ChatController(HKContext context, TranslateService tr, GptService gpt, SimService sim)
        {
            _ctx = context;
            _tr = tr;
            _gpt = gpt;
            _sim = sim;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SendMessage(string id)
        {
            ViewBag.Id = id;
            ViewData["title"] = id;
            return View(ViewBag);
        }
        //http....?key=123
        [HttpPost]
        public async Task<IActionResult> SendMessage(string id, EmbViewModel input)
        {
            //input = new Data() 
            //{
            //    Message = "test",
            //    MessageEmbedding = "test",
            //    Anser = "test"
            //};

            if (id == null)
            {
                return NotFound();
            }

            if (input.Message == null)
            {
                return await Task.FromResult<IActionResult>(BadRequest("內容不能為空!!"));
            }

            var lan = await _tr.GetLunguage(input.Message);
            lan = lan.ToString();

            //取得相對應文本的語言
            var GetEmbList = from f in _ctx.AiFiles
                             join a in _ctx.Applications on f.ApplicationId equals a.ApplicationId
                             where a.Key == input.id
                             select f;


            var DbLan = await GetEmbList.FirstOrDefaultAsync(x => x.Language != null);

            if (DbLan.Language == null)
            {
                return await Task.FromResult<IActionResult>(BadRequest("文本資料有誤!!"));
            }
            var messagetr = input.Message;
            //如果輸入語文本不同，進行翻譯
            if (lan != DbLan.Language)
            {
                messagetr = await _tr.GetTranslate(input.Message, lan, DbLan.Language);
            }

            //將問題進行嵌入
            input.MessageEmbedding = await _gpt.GetEmbedding(messagetr);

            //取得所有問題的嵌入
            var embeddings = await Task.Run(() =>
            {
                var embeddings = (from e in _ctx.Embeddings
                                 where e.AifileId == DbLan.AifileId
                                 select e).ToList();
                return embeddings;
            });

            //餘閒相關性最高的三個問題
            var SimDatas = await _sim.SimMethod(input, embeddings);

            //取得相關性最高的三個問題
            SimDatas = SimDatas.OrderBy(x => x.Similarity > 0.8).Take(5).ToList();

            //如果相關性不足，回傳錯誤
            if (SimDatas.Count == 0)
            {
                return await Task.FromResult<IActionResult>(BadRequest("無相關問題!!"));
            }

            //input.Refer = (
            //    from e in _ctx.Embeddings
            //    join s in SimDatas on e.EmbeddingId equals s.DbId
            //    select e.Qa
            //    ).ToList();
            var container = new List<string>();
            foreach ( var s in SimDatas)
            {
                var temp = await _ctx.Embeddings.FirstAsync(x => x.EmbeddingId == s.DbId);
                container.Add(temp.Qa);
            }
            input.Refer = container;
            
            input.Anser = await _gpt.GetAnser(input);

            var result = input.Anser.ToString();
            //將問題傳送至API，要求答案


            return Content(result);
        }

    }
}
