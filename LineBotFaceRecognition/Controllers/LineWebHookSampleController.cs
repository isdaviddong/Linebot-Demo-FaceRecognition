using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using Microsoft.ProjectOxford.Vision;

namespace LineBotFaceRecognition.Controllers
{
    public class LineBotWebHookController : isRock.LineBot.LineWebHookControllerBase
    {
        [Route("api/LineFaceRec")]
        [HttpPost]
        public IHttpActionResult POST()
        {
            //取得Web.config中的 app settings
            var token = System.Configuration.ConfigurationManager.AppSettings["ChannelAccessToken"];

            isRock.LineBot.Event LineEvent = null;
            try
            {
                //設定ChannelAccessToken(或抓取Web.Config)
                this.ChannelAccessToken = token;
                //取得Line Event(本例是範例，因此只取第一個)
                LineEvent = this.ReceivedMessage.events.FirstOrDefault();
                //配合Line verify 
                if (LineEvent.replyToken == "00000000000000000000000000000000") return Ok();
                //回覆訊息
                if (LineEvent.type == "message")
                {
                    if (LineEvent.message.type == "image") //收到圖片
                    {
                        //辨識與繪製圖片
                        var Messages = ProcessImage(LineEvent, token);

                        //一次把集合中的多則訊息回覆給用戶
                        this.ReplyMessage(LineEvent.replyToken, Messages);
                    }
                    else
                        this.ReplyMessage(LineEvent.replyToken, "這是展示人臉辨識的LINE Bot，請拍一張有人的照片給我唷...");
                }
                //response OK
                return Ok();
            }
            catch (Exception ex)
            {
                //如果發生錯誤，傳訊息給Admin
                this.ReplyMessage(LineEvent.replyToken, "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
        }

        /// <summary>
        /// 處理照片
        /// </summary>
        /// <param name="LineEvent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private List<isRock.LineBot.MessageBase> ProcessImage(isRock.LineBot.Event LineEvent, string token)
        {
            //web.config
            var ComputerVisionServiceKey = System.Configuration.ConfigurationManager.AppSettings["ComputerVisionServiceKey"];
            var ComputerVisionServiceEndpoint = System.Configuration.ConfigurationManager.AppSettings["ComputerVisionServiceEndpoint"];
            string Msg = "";

            //取得照片   
            //從LineEvent取得用戶上傳的圖檔bytes
            var byteArray = isRock.LineBot.Utility.GetUserUploadedContent(LineEvent.message.id, token);
            //取得圖片檔案FileStream, 分別作為繪圖與分析用
            Stream MemStream1 = new MemoryStream(byteArray);
            Stream MemStream2 = new MemoryStream(byteArray);
            //繪圖用
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(MemStream1);
            Graphics g = Graphics.FromImage(bmp);
            //ComputerVision instance
            var visionClient = new Microsoft.ProjectOxford.Vision.VisionServiceClient(
               ComputerVisionServiceKey, ComputerVisionServiceEndpoint);
            //分析用
            using (MemStream2)
            {
                //分析圖片
                var Results = visionClient.AnalyzeImageAsync(
                    MemStream2, new VisualFeature[] { VisualFeature.Faces, VisualFeature.Description }).Result;
                //分別保存性別數量
                int isM = 0, isF = 0;
                //如果找到臉，就畫方框標示出來
                foreach (var item in Results.Faces)
                {
                    var faceRect = item.FaceRectangle;
                    //畫框
                    g.DrawRectangle(
                                new Pen(Brushes.Red, 3),
                                new Rectangle(faceRect.Left, faceRect.Top,
                                    faceRect.Width, faceRect.Height));
                    //在方框旁邊顯示年紀
                    var age = 0;
                    if (item.Gender.StartsWith("F")) age = item.Age - 2; else age = item.Age;
                    //劃出數字
                    g.DrawString(age.ToString(), new Font(SystemFonts.DefaultFont.FontFamily, 24, FontStyle.Bold),
                        new SolidBrush(Color.Black),
                        faceRect.Left + 3, faceRect.Top + 3);
                    //紀錄性別數量
                    if (item.Gender.StartsWith("M"))
                        isM += 1;
                    else
                        isF += 1;
                }
                //圖片分析結果
                Msg += $"\n圖片說明：\n{Results.Description.Captions[0].Text}";

                //如果update了照片，則顯示新圖
                if (Results.Faces.Count() > 0)
                {
                    Msg += String.Format("\n找到{0}張臉, \n{1}男 {2}女", Results.Faces.Count(), isM, isF);
                }
            }

            string ImgurURL = "";
            using (MemoryStream m = new MemoryStream())
            {
                bmp.Save(m, System.Drawing.Imaging.ImageFormat.Png);
                ImgurURL = UploadImage2Imgur(m.ToArray());
            }

            //上傳成功之後，image.Link會回傳 url
            //建立文字訊息
            isRock.LineBot.TextMessage TextMsg = new isRock.LineBot.TextMessage(Msg);
            //建立圖形訊息(用上傳後的網址)
            isRock.LineBot.ImageMessage imageMsg = new isRock.LineBot.ImageMessage(new Uri(ImgurURL), new Uri(ImgurURL));
            //建立集合
            var Messages = new List<isRock.LineBot.MessageBase>();
            Messages.Add(TextMsg);
            Messages.Add(imageMsg);

            //一次把集合中的多則訊息回覆給用戶
            return Messages;
        }

        //Upload Image to Imgur
        private string UploadImage2Imgur(byte[] bytes)
        {
            var Imgur_CLIENT_ID = System.Configuration.ConfigurationManager.AppSettings["Imgur_CLIENT_ID"];
            var Imgur_CLIENT_SECRET = System.Configuration.ConfigurationManager.AppSettings["Imgur_CLIENT_SECRET"];

            //建立 ImgurClient準備上傳圖片
            var client = new ImgurClient(Imgur_CLIENT_ID, Imgur_CLIENT_SECRET);
            var endpoint = new ImageEndpoint(client);
            IImage image;
            //上傳Imgur
            image = endpoint.UploadImageStreamAsync(new MemoryStream(bytes)).GetAwaiter().GetResult();
            return image.Link;
        }
    }
}
