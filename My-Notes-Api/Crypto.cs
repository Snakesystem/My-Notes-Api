using System.Security.Cryptography;
using System.Text;


namespace Crypto
{
    public class CryptoResult
    {
        private string key = "";
        private string result = "";

        public CryptoResult()
        {
        }

        public string GetKey()
        {
            return key;
        }

        public void SetKey(string value)
        {
            key = value;
        }

        public string GetResult()
        {
            return result;
        }

        public void SetResult(string value)
        {
            result = value;
        }
    }

    public class EncryptText
    {
        const string key2 = "S21@PEDK3y2";
        bool lastProcessError = false;
        string lastErrorMessage = "";

        public EncryptText()
        {
        }

        void resetErrorLogVar()
        {
            lastProcessError = false;
            lastErrorMessage = "";
        }

        public CryptoResult EncryptString(string data, string hashKey)
        {
            resetErrorLogVar();
            CryptoResult _result = new CryptoResult();
            try
            {
                bool useHashingKey = true;
                string _cKey = GetHashString(hashKey);
                _result.SetKey(_cKey);

                _cKey = _cKey + key2;
                _result.SetResult(Encrypt(data, _cKey, useHashingKey));
            }
            catch (Exception ex)
            {
                _result.SetKey(hashKey);
                _result.SetResult(data);
                lastProcessError = true;
                lastErrorMessage = "[EncryptString]: " + ex.Message + Environment.NewLine + ex.StackTrace;
            }
            return _result;
        }
        public string DecryptString(string data, string hashKey)
        {
            resetErrorLogVar();
            string _result = "";
            try
            {
                bool useHashingKey = true;
                string _cKey = hashKey + key2;
                _result = Decrypt(data, _cKey, useHashingKey);
            }
            catch (Exception ex)
            {
                _result = data;
                lastProcessError = true;
                lastErrorMessage = "[DecryptString]: " + ex.Message + Environment.NewLine + ex.StackTrace;
            }
            return _result;
        }

        byte[] GetHashByte(string toHashData)
        {
            resetErrorLogVar();
            byte[] keyArray;
            try
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(toHashData));
                hashmd5.Clear();
            }
            catch (Exception ex)
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(toHashData);
                lastProcessError = true;
                lastErrorMessage = "[GetHashByte]: " + ex.Message + Environment.NewLine + ex.StackTrace;
            }
            return keyArray;
        }
        string GetHashString(string toHashData)
        {
            resetErrorLogVar();
            string _result = "";
            try
            {
                byte[] keyArray = GetHashByte(toHashData);
                //_result = UTF8Encoding.UTF8.GetString(keyArray);
                _result = Convert.ToBase64String(keyArray, 0, keyArray.Length); ;
            }
            catch (Exception ex)
            {
                _result = toHashData;
                lastProcessError = true;
                lastErrorMessage = "[GetHashString]: " + ex.Message + Environment.NewLine + ex.StackTrace;
            }
            return _result;
        }

        string Encrypt(string toEncrypt, string key, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                keyArray = GetHashByte(key);
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;

            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        string Decrypt(string cipherString, string key, bool useHashing)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            if (useHashing)
            {
                keyArray = GetHashByte(key);
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;

            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public string EncryptHash(string strToEncrypt)
        {
            return get3DesEncrypt(strToEncrypt, strToEncrypt, true);
        }
        public string EncryptHash(string strToEncrypt, string strKey)
        {
            return get3DesEncrypt(strToEncrypt, strKey, false);
        }
        public string EncryptHash(string strToEncrypt, string strKey, bool isAddTail)
        {
            return get3DesEncrypt(strToEncrypt, strKey, isAddTail);
        }

        private string get3DesEncrypt(string strToEncrypt, string strKey, bool isAddTail)
        {
            string _result = strToEncrypt;
            if (isAddTail) _result = strToEncrypt + "\x08";
            byte[] _dtByte = Encoding.Unicode.GetBytes(_result);
            byte[] _key = Encoding.Unicode.GetBytes(strKey);
            byte[] _bResult;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (System.Security.Cryptography.TripleDESCryptoServiceProvider _des = new System.Security.Cryptography.TripleDESCryptoServiceProvider())
            {
                /* sample Key XOR derive
                byte[] DerKey1 = new byte[64];
                byte[] DerKey2 = new byte[64];
                for (int i = 0; i < 64; i++)
                {
                    DerKey1[i] = i < 16 ? (byte)(0x36 ^ _key[i]) : (byte)0x36; 
                    DerKey2[i] = i < 16 ? (byte)(0x5c ^ _key[i]) : (byte)0x5c;
                }
                byte[] DerKey1H = getMD5Encrypt(DerKey1);
                byte[] DerKey2H = getMD5Encrypt(DerKey2);
                byte[] fKey = new byte[24];
                for (int i = 0; i < 16; i++)
                    fKey[i] = DerKey1H[i];
                for (int i = 0; i < 8; i++)
                    fKey[16 + i] = DerKey2H[i];

                //_des.Key = fKey;
             
                 */

                _des.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }; //8 bytes, zero-ed
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(_key, null); //No salt needed here
                _des.Key = pdb.CryptDeriveKey("TripleDES", "MD5", 192, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
                pdb = null;

                //_des.Mode = System.Security.Cryptography.CipherMode.CBC;
                //_des.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
                //_des.BlockSize = 64;
                //_des.FeedbackSize = 64;
                //_des.IV = new byte[_des.BlockSize / 8];

                using (System.Security.Cryptography.ICryptoTransform t = _des.CreateEncryptor())
                {
                    _bResult = t.TransformFinalBlock(_dtByte, 0, _dtByte.Length);
                }


                //Console.WriteLine("3Des Result : " + BitConverter.ToString(_bResult));

                int codepage = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
                string tempStr = Encoding.GetEncoding(codepage).GetString(_bResult);
                byte[] _lResult = Encoding.Unicode.GetBytes(tempStr);

                //Console.WriteLine("Pre MD5 : " + BitConverter.ToString(_lResult));

                _bResult = getMD5Encrypt(_lResult);

                //Console.WriteLine("Final Result : " + BitConverter.ToString(_bResult));

                _result = BitConverter.ToString(_bResult).Replace("-", "");
                _des.Clear();
            }
            return _result;
        }
        private byte[] getMD5Encrypt(byte[] dataByte)
        {
            byte[] _result;
            using (System.Security.Cryptography.MD5 _md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
            {
                _result = _md5.ComputeHash(dataByte, 0, dataByte.Length);
                _md5.Clear();
            }
            return _result;
        }

        public static string SEncrypt(string source)
        {
            var dest = "";

            for (int i = 1; i <= source.Length; i++)
            {
                dest += (char)(270 + i - Convert.ToByte(source[source.Length - i]));
            }

            return dest;
        }
        public static string SDecrypt(string source)
        {
            var dest = "";

            for (int i = 1; i <= source.Length; i++)
            {
                dest = (char)(270 + i - Convert.ToByte(source[i - 1])) + dest;
            }

            return dest;
        }

        public static string EncryptTextini(string source)
        {
            List<byte> ba = new List<byte>(Encoding.ASCII.GetBytes(source));

            int L = source.Length;
            for (int i = 0; i < L; i++)
            {
                byte data = (byte)(33 + L + i);
                ba.Insert(i * 2, data);
            }

            return Convert.ToBase64String(ba.ToArray());
        }
        public static string DecryptTextini(string source)
        {
            List<byte> data = new List<byte>(Convert.FromBase64String(source));

            int L = data.Count;
            for (int i = 0; i < L; i++)
            {
                if (i < data.Count) data.RemoveAt(i);
            }
            return Encoding.ASCII.GetString(data.ToArray());
        }
    }
}
