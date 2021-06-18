using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Diagnostics;
using ParsnipData;
using ParsnipData.Accounts;
using ParsnipData.Logging;
using System.Data;
using System.Web;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ParsnipData.Media
{
    public class VideoData
    {
        public string OriginalFileDir { get; set; }
        public string OriginalFileName { get { return OriginalFileDir?.Split('/').Last(); } }
        public string OriginalFileExtension { get { return $".{OriginalFileDir?.Split('.').Last()}"; } }
        public string CompressedFileDir { get; set; }
        public string CompressedFileName { get { return CompressedFileDir?.Split('/').Last(); } }
        public string CompressedFileExtension { get { return $".{CompressedFileDir?.Split('.').Last()}"; } }
        public string VideoDir { get { return string.IsNullOrEmpty(CompressedFileDir) ? OriginalFileDir : CompressedFileDir; } }
        public short YScale { get; set; }
        public short XScale { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public TimeSpan Duration { get; set; }
    }
    public class Video : Media
    {
        public List<VideoThumbnail> Thumbnails;
        public override string Type { get { return "video"; } }
        public override string UploadsDir { get { return "Resources/Media/Videos/Thumbnails/"; } }
        public string VideoUploadsDir { get { return "Resources/Media/Videos/"; } }
        public VideoData VideoData { get; }
        private static readonly string[] AllowedFileExtensions = new string[] { "mp4", "m4v", "mov", "mpg" };
        public static bool IsValidFileExtension(string ext)
        {
            return AllowedFileExtensions.Contains(ext.ToLower());

        }

        new public bool IsPortrait()
        {
            if (VideoData.XScale == default || VideoData.YScale == default)
                throw new InvalidOperationException();

            return VideoData.YScale > VideoData.XScale;
        }

        new public bool IsLandscape()
        {
            if (VideoData.XScale == default || VideoData.YScale == default)
                throw new InvalidOperationException();

            return VideoData.XScale > VideoData.YScale;
        }

        #region Constructors
        public Video()
        {
            Thumbnails = new List<VideoThumbnail>();
            VideoData = new VideoData();
        }

        public Video(SqlDataReader pReader, int loggedInUserId = default) : this()
        {
            AddValues(pReader, loggedInUserId);
        }

        public Video(User uploader, HttpPostedFile videoFile)
        {
            Id = MediaId.NewMediaId();

            try
            {
                string[] videoFileDir = videoFile.FileName.Split('\\');
                string originalVideoFileName = videoFileDir.Last();
                string originalVideoFileExtension = "." + originalVideoFileName.Split('.').Last();

                string generatedFileName = $"{Id}";

                var relativeVideoDir = VideoUploadsDir + "Originals/" + generatedFileName + originalVideoFileExtension;
                var fullyQualifiedVideoDir = HttpContext.Current.Server.MapPath("~/" + relativeVideoDir);
                videoFile.SaveAs(fullyQualifiedVideoDir);

                VideoData = new VideoData();
                VideoData.OriginalFileDir = relativeVideoDir;
                VideoData.XScale = XScale;
                VideoData.YScale = YScale;
                CreatedById = uploader.Id;

                DateTimeCreated = Parsnip.AdjustedTime;
                DateTimeCaptured = DateTimeCreated;
            }
            catch (Exception ex)
            {
                new LogEntry(Log.Debug) { Text = $"There was an exception whilst uploading the video: {ex}" };
            }

        }
        #endregion

        #region CRUD
        public override bool Insert()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Parsnip.ParsnipConnectionString))
                {
                    using (var insertVideo = new SqlCommand("video_INSERT", conn))
                    {
                        insertVideo.CommandType = CommandType.StoredProcedure;

                        if(!string.IsNullOrEmpty(Title))
                            insertVideo.Parameters.AddWithValue("title", Title);

                        insertVideo.Parameters.AddWithValue("original_dir", VideoData.OriginalFileDir);
                        insertVideo.Parameters.AddWithValue("datetime_captured", DateTimeCaptured);
                        if(AlbumId != default)
                            insertVideo.Parameters.AddWithValue("media_tag_id", AlbumId);
                        insertVideo.Parameters.AddWithValue("created_by_user_id", CreatedById);
                        insertVideo.Parameters.AddWithValue("new_media_id", Id.ToString());
                        insertVideo.Parameters.AddWithValue("now", Parsnip.AdjustedTime);

                        conn.Open();
                        insertVideo.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"There was an exception whilst inserting the video: {ex}");
                return false;
            }
        }
        public static List<Video> Select()
        {
            var videos = new List<Video>();
            try
            {
                using (var conn = new SqlConnection(Parsnip.ParsnipConnectionString))
                {
                    using (SqlCommand selectVideo = new SqlCommand("video_SELECT", conn))
                    {
                        selectVideo.CommandType = CommandType.StoredProcedure;

                        conn.Open();
                        using (SqlDataReader reader = selectVideo.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                var video = new Video();
                                video.AddValues(reader);
                                videos.Add(video);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"There was an exception whilst getting all videos: {ex}");
            }

            return videos;
        }

        public new static Video Select(MediaId mediaId, int loggedInUserId)
        {
            Video video = null;
            try
            {
                using (var conn = new SqlConnection(Parsnip.ParsnipConnectionString))
                {
                    using (SqlCommand selectVideo = new SqlCommand("video_SELECT_WHERE_media_id", conn))
                    {
                        selectVideo.CommandType = CommandType.StoredProcedure;

                        selectVideo.Parameters.Add(new SqlParameter("media_id", mediaId.ToString()));
                        selectVideo.Parameters.Add(new SqlParameter("logged_in_user_id", loggedInUserId));

                        conn.Open();
                        using (SqlDataReader reader = selectVideo.ExecuteReader())
                        {

                            while (reader.Read())
                            {
                                video = new Video();
                                video.AddValues(reader, loggedInUserId);
                            }

                            if (video != null)
                            {
                                reader.NextResult();

                                video.MediaTagPairs = new List<MediaTagPair>();
                                while (reader.Read())
                                {
                                    var mediaTag = new MediaTagPair(reader);
                                    video.MediaTagPairs.Add(mediaTag);
                                }

                                reader.NextResult();

                                video.MediaUserPairs = new List<MediaUserPair>();
                                while (reader.Read())
                                {
                                    var mediaUserPair = new MediaUserPair(reader);
                                    video.MediaUserPairs.Add(mediaUserPair);
                                }

                                reader.NextResult();

                                video.Thumbnails = new List<VideoThumbnail>();
                                while (reader.Read())
                                {
                                    var videoThumbnail = new VideoThumbnail(reader);
                                    video.Thumbnails.Add(videoThumbnail);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"There was an exception whilst getting video data: {ex}");
            }
            return video;
        }

        public static Video SelectOldestUncompressed()
        {
            var oldestUncompressedVideo = new Video();
            using(var conn = new SqlConnection(ParsnipData.Parsnip.ParsnipConnectionString))
            {
                using(var selectUncompressed = new SqlCommand("video_SELECT_WHERE_uncompressed", conn))
                {
                    selectUncompressed.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using(var reader = selectUncompressed.ExecuteReader())
                    {
                        if(reader.Read())
                        {
                            oldestUncompressedVideo = new Video(reader);
                        }
                    }
                }
            }
            return oldestUncompressedVideo;
        }

        public bool UpdateOriginalDir()
        {
            if (Id != default)
            {
                try
                {
                    using (var conn = new SqlConnection(ParsnipData.Parsnip.ParsnipConnectionString))
                    {
                        using (var updateMedia = new SqlCommand("video_UPDATE_original_dir", conn))
                        {
                            updateMedia.CommandType = CommandType.StoredProcedure;

                            updateMedia.Parameters.AddWithValue("media_id", Id.ToString());

                            if (!string.IsNullOrEmpty(VideoData.OriginalFileDir))
                                updateMedia.Parameters.AddWithValue("original_dir", VideoData.OriginalFileDir);

                            conn.Open();
                            updateMedia.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    string error =
                        string.Format("There was an error whilst updating media: {0}", e);
                    Debug.WriteLine(error);
                    new LogEntry(Log.General) { Text = error };
                    return false;
                }
                return true;
            }
            else
            {
                throw new System.InvalidOperationException(
                    "Media cannot be updated. Media must be inserted into the database before it can be updated!");
            }
        }

        public bool UpdateMetadata()
        {
            if (Id != default)
            {
                try
                {
                    using (var conn = new SqlConnection(ParsnipData.Parsnip.ParsnipConnectionString))
                    {
                        using (var updateMedia = new SqlCommand("video_UPDATE", conn))
                        {
                            updateMedia.CommandType = CommandType.StoredProcedure;

                            updateMedia.Parameters.AddWithValue("media_id", Id.ToString());

                            if (!string.IsNullOrEmpty(VideoData.CompressedFileDir))
                                updateMedia.Parameters.AddWithValue("compressed_dir", VideoData.CompressedFileDir);

                            if (VideoData.XScale != default)
                                updateMedia.Parameters.AddWithValue("x_scale", VideoData.XScale);

                            if (VideoData.YScale != default)
                                updateMedia.Parameters.AddWithValue("y_scale", VideoData.YScale);

                            if (VideoData.Duration != default)
                                updateMedia.Parameters.AddWithValue("duration", VideoData.Duration.Ticks);

                            if (Status != null)
                                updateMedia.Parameters.AddWithValue("status", Status.ToString());

                            conn.Open();
                            updateMedia.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception e)
                {
                    string error =
                        string.Format("There was an error whilst updating media: {0}", e);
                    Debug.WriteLine(error);
                    new LogEntry(Log.General) { Text = error };
                    return false;
                }
                return true;
            }
            else
            {
                throw new System.InvalidOperationException(
                    "Media cannot be updated. Media must be inserted into the database before it can be updated!");
            }

        }

        public void DeleteAllThumbnails()
        {
            try
            {
                using (var conn = new SqlConnection(Parsnip.ParsnipConnectionString))
                {
                    using(var deleteAllThumbnails = new SqlCommand("video_thumbnail_DELETE_WHERE_media_id", conn))
                    {
                        deleteAllThumbnails.CommandType = CommandType.StoredProcedure;

                        deleteAllThumbnails.Parameters.AddWithValue("media_id", Id.ToString());

                        conn.Open();
                        deleteAllThumbnails.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                new LogEntry(Log.Debug) { Text = $"There was an error whilst deleting video thumbnails for video {Id}: {ex}" };
            }
        }

        protected override bool AddValues(SqlDataReader reader, int loggedInUserId = default)
        {
            try
            {
                Id = new MediaId(reader[0].ToString());

                if (reader[1] != DBNull.Value && !string.IsNullOrEmpty(reader[1].ToString()) && !string.IsNullOrWhiteSpace(reader[1].ToString()))
                {
                    Title = reader[1].ToString().Trim();
                }

                if (reader[2] != DBNull.Value && !string.IsNullOrEmpty(reader[2].ToString()) && !string.IsNullOrWhiteSpace(reader[2].ToString()))
                    Description = reader[2].ToString().Trim();

                if (reader[3] != DBNull.Value && !string.IsNullOrEmpty(reader[3].ToString()) && !string.IsNullOrWhiteSpace(reader[3].ToString()))
                    Alt = reader[3].ToString().Trim();

                DateTimeCaptured = Convert.ToDateTime(reader[4]);
                DateTimeCreated = Convert.ToDateTime(reader[5]);

                if (reader[6] != DBNull.Value && !string.IsNullOrEmpty(reader[6].ToString()) && !string.IsNullOrWhiteSpace(reader[6].ToString()))
                    VideoData.XScale = (short)reader[6];

                if (reader[7] != DBNull.Value && !string.IsNullOrEmpty(reader[7].ToString()) && !string.IsNullOrWhiteSpace(reader[7].ToString()))
                    VideoData.YScale = (short)reader[7];

                if (reader[8] != DBNull.Value && !string.IsNullOrEmpty(reader[8].ToString()) && !string.IsNullOrWhiteSpace(reader[8].ToString()))
                    VideoData.OriginalFileDir = reader[8].ToString().Trim();

                if (reader[9] != DBNull.Value && !string.IsNullOrEmpty(reader[9].ToString()) && !string.IsNullOrWhiteSpace(reader[9].ToString()))
                    VideoData.CompressedFileDir = reader[9].ToString().Trim();


                if (reader[10] != DBNull.Value && !string.IsNullOrEmpty(reader[10].ToString()) && !string.IsNullOrWhiteSpace(reader[10].ToString()))
                    XScale = (short)reader[10];

                if (reader[11] != DBNull.Value && !string.IsNullOrEmpty(reader[11].ToString()) && !string.IsNullOrWhiteSpace(reader[11].ToString()))
                    YScale = (short)reader[11];

                if (reader[12] != DBNull.Value && !string.IsNullOrEmpty(reader[12].ToString()) && !string.IsNullOrWhiteSpace(reader[12].ToString()))
                    Original = reader[12].ToString().Trim();

                if (reader[13] != DBNull.Value && !string.IsNullOrEmpty(reader[13].ToString()) && !string.IsNullOrWhiteSpace(reader[13].ToString()))
                    Compressed = reader[13].ToString().Trim();

                if (reader[14] != DBNull.Value && !string.IsNullOrEmpty(reader[14].ToString()) && !string.IsNullOrWhiteSpace(reader[14].ToString()))
                    Placeholder = reader[14].ToString().Trim();

                CreatedById = (int)reader[15];

                try
                {
                    if (reader[17] != DBNull.Value && !string.IsNullOrEmpty(reader[17].ToString()) && !string.IsNullOrWhiteSpace(reader[17].ToString()))
                        AlbumId = (int)reader[17];
                }
                catch (IndexOutOfRangeException)
                {
                    //These values are not necessarily returned in all queries
                }

                try
                {
                    if (reader[19] == DBNull.Value && string.IsNullOrEmpty(reader[19].ToString()) &&
                        string.IsNullOrWhiteSpace(reader[19].ToString()))

                    {
                        if (loggedInUserId != default)
                        {
                            MyMediaShare = new MediaShare(Id, loggedInUserId);
                            MyMediaShare.Insert();
                        }
                    }
                    else
                    {
                        MyMediaShare = new MediaShare(new MediaShareId((string)reader[19]), (int)reader[20], Convert.ToDateTime(reader[21]), default, new MediaId((string)reader[0]));
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    //These values are not necessarily returned in all queries
                }

                Status = new MediaStatus(reader[22].ToString().Trim());

                try
                {
                    if (reader[23] != DBNull.Value && !string.IsNullOrEmpty(reader[23].ToString()))
                        SearchTerms = reader[23].ToString().Trim();
                }
                catch (IndexOutOfRangeException)
                {

                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("There was an error whilst reading the Video's values: " + ex);
                return false;
            }
        }
        #endregion
    }
}
