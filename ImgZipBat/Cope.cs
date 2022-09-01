using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImgZipBat
{
    public class Cope
    {
        public static string sourcePath; // 源文件目录
        public static int quality; // 压缩质量
        public static int way; // 覆盖方式，1,2,3 代替
        // 1 直接覆盖
        // 2 重命名源文件
        // 3 重命名压缩后文件
        // 4 源文件转移到指定位置
        // 5 压缩后的文件输出到指定位置
        public static string arg; // 另存路径或命名规则
        public static bool isJpg; // 是否全部保存成jpg
        public static bool isClose = true; // 是否关闭

        public delegate void DisposeProgress(int current, int total,string fileName, string info);
        public event DisposeProgress DisposeProgressEvent;
        public void Dispose()
        {
            Thread.Sleep(200);
            DisposeProgressClick(0, 0, "准备", "采集所有图片文件......");

            List<string> filesPath = new List<string>();
            // 获取所有文件
            GetAllFiles(sourcePath, filesPath);

            int total = filesPath.Count;
            DisposeProgressClick(0, 0, "准备", "采集结束，开始处理......");

            int index = 0;
            int success = 0;
            foreach (string s in filesPath)
            {
                index++;
                if (isClose)
                {
                    DisposeProgressClick(-2,success, "已停止", "手动停止处理......");
                    return;
                }

                DisposeProgressClick(index, total, s, "正在处理");

                string targetFile = "";
                string sourceFile = s;
                if (way == 1)
                {
                    // 覆盖原文件
                    targetFile = sourceFile +"_.jpg" ;
                }
                else if (way == 2)
                {
                    // 重命名源文件

                }
                else if (way == 3)
                {
                    // 重命名压缩后文件

                }
                else if (way == 4)
                {
                    // 源文件转移到指定位置

                }
                else if (way == 5)
                {
                    // 压缩后的文件输出到指定位置

                }

                if (!File.Exists(sourceFile))
                {
                    DisposeProgressClick(index, total, "源文件不存在", "错误");
                    continue;
                }

                // 获取源文件大小
                FileInfo sourceFileInfo = new FileInfo(sourceFile);
                long sourceFileInfoSize = sourceFileInfo.Length;

                ImgCompress(sourceFile, targetFile, quality);

                // 获取压缩后文件大小
                FileInfo targetFileInfo = new FileInfo(targetFile);
                long targetFileInfoSize = targetFileInfo.Length;

                // 计算压缩率
                int cr = 100;
                if (sourceFileInfoSize != 0) 
                {
                    cr = (int)(targetFileInfoSize / sourceFileInfoSize * 100);
                }

                DisposeProgressClick(index, total, "压缩率：" + cr + "%", "成功");

                success++;
            }

            isClose = true;
            DisposeProgressClick(-1, success, "完成", "");
        }

        private void DisposeProgressClick(int current, int total, string fileName, string info)
        {
            if (DisposeProgressEvent != null)
            {
                DisposeProgressEvent(current, total, fileName, info);
            }
        }

        //压缩方法
        private void ImgCompress(string source, string target, long level)
        {
            GetPicThumbnail(source, target, (int)level);
            if (false)
            {

                Stream imgStream = null;
                Stream outStream = null;
                try
                {
                    imgStream = new FileStream(source, FileMode.Open, FileAccess.ReadWrite);
                    Image img = Image.FromStream(imgStream);

                    ImageFormat imgFormat = img.RawFormat;
                    ImageCodecInfo codecInfo = GetEncoder(imgFormat);

                    EncoderParameters encoderParams = new EncoderParameters();
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, level);

                    if (source.Equals(target))
                    {
                        imgStream.Position = 0;
                        img.Save(imgStream, codecInfo, encoderParams);
                    }
                    else
                    {
                        outStream = new FileStream(target, FileMode.Create, FileAccess.Write);
                        img.Save(outStream, codecInfo, encoderParams);
                    }
                    img.Dispose();
                }
                catch (IOException e)
                {
                    DisposeProgressClick(0, 0, "出错", e.Message);

                    Tools.Log("{0}", e);
                }
                finally
                {
                    if (imgStream != null)
                    {
                        imgStream.Close();
                    }

                    if (outStream != null)
                    {
                        outStream.Close();
                    }
                }
            }
        }

        public static bool GetPicThumbnail(string sFile, string outPath, int flag)
        {
            Bitmap iSource = new Bitmap(sFile);
            ImageFormat tFormat = iSource.RawFormat;
            //以下代码为保存图片时，设置压缩质量 
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100 
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    // 保存为源格式
                    Tools.Log("{0}，{1}，保存为源",ep, jpegICIinfo);
                    iSource.Save(outPath, jpegICIinfo, ep);//dFile是压缩后的新路径 
                }
                else
                {
                    // 保存为jpg
                    Tools.Log("{0}，{1}，保存为jpg", tFormat);
                    iSource.Save(outPath, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (iSource != null)
                {
                    iSource.Dispose();
                }
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定目录下的所有符合条件的文件存储到列表中
        /// </summary>
        /// <param name="path"></param>
        /// <param name="list"></param>
        public void GetAllFiles(string path, List<string> list)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                // BMP、GIF、EXIF、JPG、PNG 和 TIFF
                string[] extensions = new string[] { "jpg", "bmp","gif","exif","jpeg","png","tiff", "tif" };
                foreach (string extension in extensions)
                {
                    if (file.Extension.ToLower().Contains(extension))
                    {
                        list.Add(file.FullName);
                        DisposeProgressClick(0, 0, file.FullName,"待处理文件");
                        break;
                    }
                }
            }

            foreach (DirectoryInfo dirInfo in directoryInfo.GetDirectories())
            {
                GetAllFiles(dirInfo.FullName, list);
            }
        }


    }
}
