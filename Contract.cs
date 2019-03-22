using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Util {
    [DataContract]
    public class ResponseVkRegister {
        [DataMember]
        public ResponseVkRegisterData response;
    }
    [DataContract]
    public class ResponseVkRegisterData {
        [DataMember]
        public string ts;
        [DataMember]
        public string new_ts;
        [DataMember]
        public string server;
        [DataMember]
        public string key;
        [DataMember]
        public string upload_url;
        [DataMember]
        public int album_id;
        [DataMember]
        public int user_id;

    }

    [DataContract]
    public class ResponseVk {

        [DataMember]
        public string ts;
        [DataMember]
        public string new_ts;
        [DataMember]
        public Update[] updates;
        [DataMember]
        public int failed;
    }

    [DataContract]
    public class Update {
        [DataMember]
        public string type;
        [DataMember]
        public ObjectVK @object;
    }
    [DataContract]
    public class ObjectVK {
        [DataMember]
        public int id;
        [DataMember]
        public int date;
        [DataMember]
        public int peer_id;
        [DataMember]
        public int from_id; 
        [DataMember]
        public string text;
        [DataMember]
        public int random_id;
        
    }
    [DataContract]
    public class ResponseVkPhotoUpload {

        [DataMember]
        public string server;
        [DataMember]
        public string photo;
        [DataMember]
        public string hash;

    }

    [DataContract]
    public class ResponseVkPhotoSave {

        [DataMember]
        public ResponseVkPhotoSaveData[] response;
    }

    [DataContract]
    public class ResponseVkPhotoSaveData {

        [DataMember]
        public string id;
        [DataMember]
        public int album_id;
        [DataMember]
        public int owner_id;
        [DataMember]
        public object[] sizes;
        [DataMember]
        public string text;
        [DataMember]
        public int date;
        [DataMember]
        public string access_key;
    }

    [DataContract]
    public class ResponseImgflip {
        [DataMember]
        public bool success;
        [DataMember]
        public string error_message;
        [DataMember]
        public ObjectImgflip data;
    }
    [DataContract]
    public class ObjectImgflip {
        [DataMember]
        public string url;
        [DataMember]  
        public string page_url;
    }
}