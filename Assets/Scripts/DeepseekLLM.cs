using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DeepseekLLM : LLM
{
    public DeepseekLLM()
    {
        url = "https://api.moonshot.cn/v1/chat/completions";
    }

    /// <summary>
    /// api key
    /// </summary>
    [SerializeField] private string api_key;
    /// <summary>
    /// AI设定
    /// </summary>
    public string m_SystemSetting = string.Empty;
    /// <summary>
    /// gpt-3.5-turbo
    /// </summary>
    public string m_gptModel = "moonshot-v1-8k";


    private void Start()
    {
        //运行时，添加AI设定
        m_DataList.Add(new SendData("system", m_SystemSetting));
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <returns></returns>
    public override void PostMsg(string _msg, Action<string> _callback)
    {
        base.PostMsg(_msg, _callback);
    }

    /// <summary>
    /// 调用接口
    /// </summary>
    /// <param name="_postWord"></param>
    /// <param name="_callback"></param>
    /// <returns></returns>
    public override IEnumerator Request(string _postWord, System.Action<string> _callback)
    {
        stopwatch.Restart();
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            PostData _postData = new PostData
            {
                model = m_gptModel,
                messages = m_DataList
            };

            string _jsonText = JsonUtility.ToJson(_postData).Trim();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", string.Format("Bearer {0}", api_key));

            yield return request.SendWebRequest();

            Debug.Log("ChatGPT 发送");

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("ChatGPT 正常返回");

                string _msgBack = request.downloadHandler.text;
                MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msgBack);
                if (_textback != null && _textback.choices.Count > 0)
                {

                    string _backMsg = _textback.choices[0].message.content;
                    //添加记录
                    m_DataList.Add(new SendData("assistant", _backMsg));
                    _callback(_backMsg);
                }

            }
            else
            {
                long statusCode = request.responseCode;
                Debug.LogError(request.responseCode);
                Debug.LogError(request.error);
                string _msgBack = request.downloadHandler.text;
                Debug.LogError(_msgBack);
            }

            stopwatch.Stop();
            Debug.Log("ChatGPT 耗时：" + stopwatch.Elapsed.TotalSeconds);
        }
    }

    #region 数据包

    [Serializable]
    public class PostData
    {
        [SerializeField] public string model;
        [SerializeField] public List<SendData> messages;
        [SerializeField] public float temperature = 0.7f;
    }

    [Serializable]
    public class MessageBack
    {
        public string id;
        public string created;
        public string model;
        public List<MessageBody> choices;
    }
    [Serializable]
    public class MessageBody
    {
        public Message message;
        public string finish_reason;
        public string index;
    }
    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    #endregion

}
