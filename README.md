Linebot-Demo-FaceRecognition
===

此Line bot範例為使用 LineBotSDK 建立的 <br>
人臉辨識機器人 <br>
請直接拍照或上傳一張圖片給此LINE Bot，<br>
看看有何結果 ? <br>

使用畫面
===
 ![](https://i.imgur.com/QQbP3eX.png)

測試 - 想要試玩看看?
===
您可以用LINE 搜尋 @jtv0835u 將其加入好友即可測試 <br>
或掃描QR Code <br>
![](https://i.imgur.com/5b7b4n0.png)

如何佈署專案
===
* 請 clone 之後，修改 web.config 中的 ChannelAccessToken
```xml
  <appSettings>
    <add key="ChannelAccessToken" value="~~~ 請換成你的ChannelAccessToken ~~~" />
    <add key="Imgur_CLIENT_ID" value="~~~ Imgur_CLIENT_ID ~~~" />
    <add key="Imgur_CLIENT_SECRET" value="~~~ Imgur_CLIENT_SECRET ~~~" />
    <add key="ComputerVisionServiceKey" value="~~~ Computer Vision Service Key ~~~" />
    <add key="ComputerVisionServiceEndpoint" value="~~~ 請換成你的Computer Vision Service Endpoint ~~~" />
  </appSettings>
```
* 為了便於除錯，請修改 LineAccountBookController.cs 中的 Admin User Id
```csharp
   catch (Exception ex)
            {
                //回覆訊息
                this.PushMessage("請改成你自己的Admin User Id", "發生錯誤:\n" + ex.Message);
                //response OK
                return Ok();
            }
```
* 建議使用Ngrok進行測試 <br/>
(可參考 https://youtu.be/kCga1_E-ijs ) 
* LINE Bot後台的WebHook設定，其位置為 Http://你的domain/api/LineFaceRec

資料庫
===
* 本範例沒有使用資料庫
 
注意事項
===
由於這只是一個範例，我們盡可能用最簡單的方式來開發。 <br/>
使用source code需要先申請 MS Computer Vision與Imgur API帳號

線上課程 與 電子書 
===
LineBotSDK線上教學課程: <br/>
https://www.udemy.com/line-bot <br/>
 <br/>
電子書購買位置(包含範例完整說明): <br/>
https://www.pubu.com.tw/ebook/103305 <br/>

