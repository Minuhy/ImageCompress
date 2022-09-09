using Microsoft.VisualBasic.FileIO;
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

                try
                {
                    // 获取源文件大小
                    FileInfo sourceFileInfo = new FileInfo(sourceFile);
                    long sourceFileInfoSize = sourceFileInfo.Length;

                    targetFile = sourceFileInfo.DirectoryName + "\\" + "_target_._temp";

                    if (!File.Exists(sourceFile))
                    {
                        DisposeProgressClick(index, total, "源文件不存在", "错误");
                        continue;
                    }

                    if (File.Exists(targetFile))
                    {
                        DisposeProgressClick(index, total, "缓存已存在", "警告");
                        File.Delete(targetFile);
                    }

                    ImgCompress(sourceFile, targetFile, quality);

                    if (!File.Exists(targetFile))
                    {
                        DisposeProgressClick(index, total, "压缩图片失败", "错误");
                        continue;
                    }

                    // 获取压缩后文件大小
                    FileInfo targetFileInfo = new FileInfo(targetFile);
                    long targetFileInfoSize = targetFileInfo.Length;



                    if (way == 1)
                    {
                        // 覆盖原文件
                        string sourceName = sourceFileInfo.Name;
                        if (sourceFileInfo.Exists)
                        {
                            File.Delete(sourceFile);
                        }

                        if (targetFileInfo.Exists)
                        {
                            FileSystem.RenameFile(targetFile, sourceName);
                        }
                    }
                    else if (way == 2)
                    {
                        // 重命名源文件
                        string sourceName = sourceFileInfo.Name;
                        if (sourceFileInfo.Exists)
                        {
                            // 文件名 不包含后缀名
                            string name = sourceFileInfo.Name.Substring(0, sourceFileInfo.Name.Length - sourceFileInfo.Extension.Length);
                            // 插入到表达式，得到新文件名
                            name = arg.Replace("*", name);
                            if (isJpg)
                            {
                                name += ".jpg";
                            }
                            else
                            {
                                name += sourceFileInfo.Extension;
                            }

                            //sourceFile 是文件完整路径，新的名称要包含文件后缀名
                            FileSystem.RenameFile(sourceFile, name);
                        }

                        if (targetFileInfo.Exists)
                        {
                            FileSystem.RenameFile(targetFile, sourceName);
                        }
                    }
                    else if (way == 3)
                    {
                        // 重命名压缩后文件

                        // 文件名 不包含后缀名
                        string name = sourceFileInfo.Name.Substring(0, sourceFileInfo.Name.Length - sourceFileInfo.Extension.Length);
                        // 插入到表达式，得到新文件名
                        name = arg.Replace("*", name);
                        if (isJpg)
                        {
                            name += ".jpg";
                        }
                        else
                        {
                            name += sourceFileInfo.Extension;
                        }

                        if (targetFileInfo.Exists)
                        {
                            FileSystem.RenameFile(targetFile, name);
                        }
                    }
                    else if (way == 4)
                    {
                        // 源文件转移到指定位置
                        if (sourceFileInfo.Exists)
                        {
                            // 文件名 不包含后缀名
                            string name = sourceFileInfo.Name.Substring(0, sourceFileInfo.Name.Length - sourceFileInfo.Extension.Length);
                            if (isJpg)
                            {
                                name += ".jpg";
                            }
                            else
                            {
                                name += sourceFileInfo.Extension;
                            }
                            string destFile = arg + "\\" +name;
                            if (MoveFolder(sourceFile, destFile))
                            {
                                // 覆盖原文件
                                string sourceName = sourceFileInfo.Name;
                                if (sourceFileInfo.Exists)
                                {
                                    File.Delete(sourceFile);
                                }

                                if (targetFileInfo.Exists)
                                {
                                    FileSystem.RenameFile(targetFile, sourceName);
                                }
                            }
                            else
                            {
                                DisposeProgressClick(index, total, "源文件移动失败", "错误");
                                continue;
                            }
                        }
                        else
                        {
                            DisposeProgressClick(index, total, "源文件不存在", "错误");
                            continue;
                        }
                    }
                    else if (way == 5)
                    {
                        // 压缩后的文件输出到指定位置
                        if (targetFileInfo.Exists)
                        {
                            // 文件名 不包含后缀名
                            string name = sourceFileInfo.Name.Substring(0, sourceFileInfo.Name.Length - sourceFileInfo.Extension.Length);
                            if (isJpg)
                            {
                                name += ".jpg";
                            }
                            else
                            {
                                name += sourceFileInfo.Extension;
                            }
                            string destFile = arg + "\\" + name;
                            if (!MoveFolder(targetFile, destFile))
                            {
                                DisposeProgressClick(index, total, "源文件移动失败", "错误");
                                continue;
                            }
                        }
                        else
                        {
                            DisposeProgressClick(index, total, "目标文件不存在", "错误");
                            continue;
                        }
                    }


                    // 计算压缩率
                    double cr = 100;
                    if (sourceFileInfoSize != 0)
                    {
                        Tools.Log(targetFile + " 压缩前大小：" + sourceFileInfoSize + " 压缩后大小：" + targetFileInfoSize);
                        cr = targetFileInfoSize * 100.0 / sourceFileInfoSize;
                    }

                    DisposeProgressClick(index, total, "压缩率：" + cr.ToString("0.00") + "%", "成功");

                    success++;
                }catch(Exception ex)
                {
                    DisposeProgressClick(index, total, "出错", ex.Message);
                }
                finally
                {
                    // 缓存文件
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }
                }
            }

            isClose = true;
            DisposeProgressClick(-1, success, "完成", "");
        }

        public bool MoveFolder(string sourcePath, string destPath)
        {
            if (File.Exists(sourcePath))
            {
                if (!File.Exists(destPath))
                {
                    try
                    {
                        File.Move(sourcePath, destPath);
                    }catch(Exception ex)
                    {
                        Tools.Log("移动文件失败：" + ex.Message);
                        return false;
                    }
                }
                else
                {
                    Tools.Log("移动文件失败：目标文件已存在");
                    return false;
                }
            }
            else
            {
                Tools.Log("移动文件失败：源文件不存在" );
                return false;
            }

            return true;
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
            catch(Exception ex)
            {
                Tools.Log("出错：", ex.Message);
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

        protected void CompressAndSave(System.Drawing.Image img, string path, string extension)
        {
            //try
            //{
            //    // If jpg is extension then remove png equivalent of it
            //    // because if extension is same it'll be overridden
            //    string delFilePath = path;
            //    if (extension == ".png")
            //    {
            //        delFilePath = delFilePath.Substring(0, delFilePath.Length - 3) + "jpg";
            //    }
            //    else
            //    {
            //        delFilePath = delFilePath.Substring(0, delFilePath.Length - 3) + "png";
            //    }
            //    if (System.IO.File.Exists(delFilePath))
            //    {
            //        System.IO.File.Delete(delFilePath);
            //    }
            //}
            //catch { }
            if (extension == ".jpg" || extension == ".jpeg")
            {
                using (Bitmap bitmap = new Bitmap(img))
                {
                    ImageCodecInfo imageEncoder = null;
                    imageEncoder = GetEncoder(ImageFormat.Jpeg);
                    // Create an Encoder object based on the GUID  
                    // for the Quality parameter category.  
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

                    // Create an EncoderParameters object.  
                    // An EncoderParameters object has an array of EncoderParameter  
                    // objects. In this case, there is only one  
                    // EncoderParameter object in the array.  
                    EncoderParameters encodingParams = new EncoderParameters(1);

                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                    encodingParams.Param[0] = myEncoderParameter;
                    bitmap.Save(path, imageEncoder, encodingParams);
                }
            }
            else
            {
                // 包：ImageProcessor
                //var quantizer = new WuQuantizer();
                //using (var bitmap = new Bitmap(img))
                //{
                //    using (var quantized = quantizer.QuantizeImage(bitmap)) //, alphaTransparency, alphaFader))
                //    {
                //        quantized.Save(path, ImageFormat.Png);
                //    }
                //}
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
