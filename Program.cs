using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
//using System.Xml.Linq;
//using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Net;
//using System.Text;
//using System.Text.Encoding.CodePages;
using Util;

namespace VKbot
{
    class Program
    {
        
        
        static string _imgFlipUsername = "VityaBot";
        static string _imgFlipPassword = "vityaBot1!";
        static string _apiVersion = "5.92";
        static string _version = "2";//depricated
        static string _wait = "25";
        static string _apiUrl = "https://api.vk.com/";
        static string _apiAccessToken = "0c29646fefcc442729f323eaf428f999dba1bcc95abfe3da03d0459c7b55fe6b965a59585b14c7a1c24af";
        static string _groupId = "179992947";
        static int _vityaId = 212515973;
        //static string _keyPhrase = "говорит:";
        static string[] _memePhrases = new[] {"!", "говорит:"};
        static string _addPhrase = "/add";
        static List<string> _memeIds = new List<string>{
                "140165357",
                "131429347",
                "136969882",
                "149017264",
                "156217874",
                "164453195",
                "155944363",
                "159322555",
                "154375976",
                "162536150",
                "167838922",
                "143051342",
                "150065404",
                "156519791",
                "175797735",//new
                "175803022",
                "175803056",
                "175803091",
                "175803124",
                "175803143",
                "175803170"
            };
        static HttpClient _httpClient = new HttpClient();
        static Random random = new Random();

        static ResponseVkRegisterData chatAuth;
        static ResponseVkRegisterData photoAuth;
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            chatAuth = register();
            while(true)
            {
                try
                {
                    process();
                
                } catch (Exception ex) {
                    LogUtil.Log.infoAsync($"Received an exception during a long poll request, trying again...\n{ex}");
                    Task.Delay(5000).Wait();
                    chatAuth = register();
                }

            }
        }

        private static ResponseVkRegisterData register()
        {
            var urlBuilder = new UriBuilder(_apiUrl)
            {
                Path = "method/groups.getLongPollServer",
                Query = $"group_id={_groupId}&access_token={_apiAccessToken}&v={_apiVersion}"
            };
            try
            {
                LogUtil.Log.infoAsync($"Getting server {urlBuilder}");

                var responseBody = _httpClient.GetStringAsync(urlBuilder.Uri).Result;

                LogUtil.Log.infoAsync(responseBody);

                var responseObj = deserialize<ResponseVkRegister>(responseBody);
                return responseObj.response;
            }
            catch (Exception e)
            {
                LogUtil.Log.infoAsync(e);
                return null;
            }
        }

        private static ResponseVkRegisterData registerPhoto(int peer_id)
        {
            var urlBuilder = new UriBuilder(_apiUrl)
            {
                Path = "method/photos.getMessagesUploadServer",
                Query = $"peer_id={peer_id.ToString()}&access_token={_apiAccessToken}&v={_apiVersion}"
            };
            try
            {
                LogUtil.Log.infoAsync($"Getting server {urlBuilder}");

                var responseBody = _httpClient.GetStringAsync(urlBuilder.Uri).Result;

                LogUtil.Log.infoAsync(responseBody);

                var responseObj = deserialize<ResponseVkRegister>(responseBody);
                return responseObj.response;
            }
            catch (Exception e)
            {
                LogUtil.Log.infoAsync(e);
                return null;
            }
        }

        private static void process()
        {
            var url = $"{chatAuth.server}?act=a_check&ts={chatAuth.ts}&key={chatAuth.key}&wait={_wait}&version={_version}";
            LogUtil.Log.infoAsync(url);
            var responseBody = _httpClient.GetStringAsync(url).Result;

            LogUtil.Log.infoAsync(responseBody);

            var responseObj = deserialize<ResponseVk>(responseBody);            

            if (responseObj.failed != 0)
            {
                LogUtil.Log.infoAsync("Error received");
                switch (responseObj.failed)
                {
                    case 1:
                        chatAuth.ts = responseObj.new_ts ?? responseObj.ts;
                        LogUtil.Log.infoAsync("Ts updated");
                        break;
                    case 2:
                    case 3:
                        throw new Exception("Session expired. Reconnect to service.");
                        break;
                    default:
                        LogUtil.Log.infoAsync($"Unknown error code {responseObj.failed}");
                        throw new Exception("Unknown error");
                        break;
                }
                return;
            }

            chatAuth.ts = responseObj.ts;

            foreach(var objectVK in responseObj.updates.Where(t => t.type == "message_new" || t.type == "message_reply").Select(t => t.@object).ToList())
            {
                if (!string.IsNullOrWhiteSpace(objectVK.text)) {
                    if(objectVK.from_id == _vityaId)
                    {
                        processMeme(objectVK.peer_id, objectVK.text);
                    } else if (_memePhrases.Any(t => objectVK.text.ToLowerInvariant().StartsWith(t)))
                    {
                        foreach(var memePhrase in _memePhrases.Where(t => objectVK.text.ToLowerInvariant().StartsWith(t))) {
                            int index = objectVK.text.ToLowerInvariant().IndexOf(memePhrase);
                            if (index != -1) {
                                string text = objectVK.text.Substring(index + memePhrase.Length, objectVK.text.Length - index - memePhrase.Length);
                                processMeme(objectVK.peer_id, text);
                            }
                        }
                    } else if (objectVK.text.ToLowerInvariant().Contains(_addPhrase)) {
                        LogUtil.Log.infoAsync($"add new meme. command: {objectVK.text}");
                        var memeId = objectVK.text.ToLowerInvariant().Replace(_addPhrase, "").Trim();
                        if (!string.IsNullOrWhiteSpace(memeId)) {
                            _memeIds.Add(memeId);
                            sendMessage(peer_id: objectVK.peer_id, text: $"meme id was added. {memeId}" );
                        }
                    }

                }
            }
        }

        private static void processMeme(int peer_id, string text) {
            LogUtil.Log.infoAsync($"processMeme. peer_id: {peer_id} text{text}");
            string memeUrltest = getMeme(text);
            photoAuth = registerPhoto(peer_id);
            var photoParams = uploadPhoto(memeUrltest, peer_id);
            var photo = savePhoto(photoParams);
            sendMessage(peer_id, $"photo{photo.owner_id}_{photo.id}_{photo.access_key}");
        }

        private static String getMeme(String text)
        {

            var values = new Dictionary<string, string>
            {
                { "template_id",  _memeIds[random.Next(0,_memeIds.Count())]},
                { "username", _imgFlipUsername },
                { "password", _imgFlipPassword },
                { "text0", text }
                //,{ "text1", "" }
            };

            var content = new FormUrlEncodedContent(values);

            var response = _httpClient.PostAsync("https://api.imgflip.com/caption_image", content).Result;
            var responseBody = response.Content.ReadAsStringAsync().Result;

            LogUtil.Log.infoAsync(responseBody);
            Console.WriteLine(responseBody);
            
            var responseObj = deserialize<ResponseImgflip>(responseBody); 

            if (responseObj.success)
            {
                return responseObj.data.url;
            }
            throw new Exception($"Imgflip error:{responseObj.error_message}");
        }

        private static void sendMessage(int peer_id, string photo_id = null, string text = null)
        {

            var urlBuilder = new UriBuilder(_apiUrl)
            {
                Path = "method/messages.send",
                Query = $"group_id={_groupId}&access_token={_apiAccessToken}&v={_apiVersion}"
            };

             var values = new Dictionary<string, string>
            {
                { "random_id", DateTime.Today.ToFileTimeUtc().ToString()},
                { "peer_id", peer_id.ToString()},
                { "message", text },
                { "attachment", photo_id }
                
            };

            var content = new FormUrlEncodedContent(values);

            var response = _httpClient.PostAsync(urlBuilder.Uri, content).Result;

            var responseBody = response.Content.ReadAsStringAsync().Result;

            LogUtil.Log.infoAsync(responseBody);
        }
        private static ResponseVkPhotoUpload uploadPhoto(string url, int peer_id)
        {
            Console.WriteLine(new Uri(url).Fragment);
            var uri = new Uri(url);
             using (WebClient client = new WebClient()) 
            {
                byte[] image = client.DownloadDataTaskAsync(new Uri(url)).Result;
                
                MultipartFormDataContent form = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(image);
                imageContent.Headers.ContentType =  System.Net.Http.Headers.MediaTypeHeaderValue.Parse("multipart/form-data");
                form.Add(imageContent, "file", System.IO.Path.GetFileName(uri.LocalPath));
                HttpResponseMessage response = _httpClient.PostAsync(photoAuth.upload_url, form).Result;
                response.EnsureSuccessStatusCode();
                var responseBody = response.Content.ReadAsStringAsync().Result;

                LogUtil.Log.infoAsync(responseBody);

                var responseObj = deserialize<ResponseVkPhotoUpload>(responseBody); 
                return responseObj;
            }
        }

        private static ResponseVkPhotoSaveData savePhoto(ResponseVkPhotoUpload photoParams) {
            var urlBuilder = new UriBuilder(_apiUrl)
                {
                    Path = "method/photos.saveMessagesPhoto",
                    Query = $"access_token={_apiAccessToken}&v={_apiVersion}"
                };

                var values = new Dictionary<string, string>
                {
                    { "photo", photoParams.photo},
                    { "server", photoParams.server},
                    { "hash", photoParams.hash }
                };

                var content = new FormUrlEncodedContent(values);

                var response = _httpClient.PostAsync(urlBuilder.Uri, content).Result;

                var responseBody = response.Content.ReadAsStringAsync().Result;

                LogUtil.Log.infoAsync(responseBody);

                var responseObj = deserialize<ResponseVkPhotoSave>(responseBody);

                return responseObj.response.FirstOrDefault();
        }


        private static T deserialize<T>(string json) where T : class, new()
        {
            var result = (T)Activator.CreateInstance(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));  
            DataContractJsonSerializer ser = new DataContractJsonSerializer(result.GetType());  
            result = ser.ReadObject(ms) as T;
            ms.Close();
            return result;  
        }
    }
}
