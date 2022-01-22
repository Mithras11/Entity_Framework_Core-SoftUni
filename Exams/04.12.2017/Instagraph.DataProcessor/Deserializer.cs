using System.IO;
using System.Text;
using System.Linq;
using Instagraph.Data;
using Newtonsoft.Json;
using Instagraph.Models;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Instagraph.DataProcessor.DtoModels.Import;
using System;

namespace Instagraph.DataProcessor
{
    public class Deserializer
    {
        private static string SuccessMessage = "Successfully imported {0}.";

        private static string ErrorMessage = "Error: Invalid data.";

        public static string ImportPictures(InstagraphContext context, string jsonString)
        {
            var picturesDtos = JsonConvert.DeserializeObject<List<ImportPictureDto>>(jsonString);

            var pictures = new List<Picture>();
            var paths = new List<string>();

            var sb = new StringBuilder();


            foreach (var picDto in picturesDtos)
            {
                //check if valid via attributes
                if (!IsValid(picDto) ||           //string path is required- if it is empty an error is thrown
                    paths.Contains(picDto.Path))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;

                }

                paths.Add(picDto.Path);

                var newPic = new Picture()
                {
                    Path = picDto.Path,
                    Size = picDto.Size
                };

                pictures.Add(newPic);
                sb.AppendLine(String.Format(SuccessMessage,
                 $"Picture {newPic.Path}"));

            }

            context.Pictures.AddRange(pictures);
            context.SaveChanges();

            return sb.ToString().Trim();

        }

        public static string ImportUsers(InstagraphContext context, string jsonString)
        {
           
        }

        public static string ImportFollowers(InstagraphContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var serializer = JsonConvert.DeserializeObject<List<ImportFollowerDto>>(jsonString);

            var followersToAdd = new List<UserFollower>();

            foreach (var followerDto in serializer)
            {
                if (!IsValid(followerDto))
                {
                    sb.AppendLine(ErrorMessage);

                    continue;
                }

                var user = context.Users.FirstOrDefault(u => u.Username == followerDto.User);

                var follower = context.Users.FirstOrDefault(u => u.Username == followerDto.Follower);

                var isFollowing = followersToAdd.Any(u => u.User == user && u.Follower == follower);

                if (user == null ||
                    follower == null ||
                    isFollowing)
                {
                    sb.AppendLine(ErrorMessage);

                    continue;
                }

                var userFollower = new UserFollower()
                {
                    User = user,
                    Follower = follower,
                };

                followersToAdd.Add(userFollower);

                sb.AppendLine($"Successfully imported Follower {follower.Username}to User {user.Username}.");
            }

            context.UsersFollowers.AddRange(followersToAdd);

            context.SaveChanges();

            return sb.ToString().Trim();
        }

        public static string ImportPosts(InstagraphContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(List<ImportPostDto>), new XmlRootAttribute("posts"));

            var namespaces = new XmlSerializerNamespaces();

            namespaces.Add(string.Empty, string.Empty);

            var reader = new StringReader(xmlString);

            var postsToAdd = new List<Post>();

            using (reader)
            {
                var postDtos = (List<ImportPostDto>)serializer.Deserialize(reader);

                foreach (var postDto in postDtos)
                {
                    if (!IsValid(postDto))
                    {
                        sb.AppendLine(ErrorMessage);

                        continue;
                    }

                    var user = context.Users.FirstOrDefault(u => u.Username == postDto.User);

                    var picture = context.Pictures.FirstOrDefault(p => p.Path == postDto.Picture);

                    if (user == null || picture == null)
                    {
                        sb.AppendLine(ErrorMessage);

                        continue;
                    }

                    var post = new Post()
                    {
                        User = user,
                        Caption = postDto.Caption,
                        Picture = picture,
                        UserId = user.Id,
                        PictureId = picture.Id
                    };

                    postsToAdd.Add(post);

                    sb.AppendLine($"Successfully imported Post {post.Caption}.");
                }

                context.Posts.AddRange(postsToAdd);

                context.SaveChanges();

                return sb.ToString().Trim();
            }
        }

        public static string ImportComments(InstagraphContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(List<ImportCommentDto>), new XmlRootAttribute("comments"));

            var namespaces = new XmlSerializerNamespaces();

            namespaces.Add(string.Empty, string.Empty);

            var reader = new StringReader(xmlString);

            var commentsToAdd = new List<Comment>();

            using (reader)
            {
                var commentDtos = (List<ImportCommentDto>)serializer.Deserialize(reader);

                foreach (var commentDto in commentDtos)
                {
                    if (!IsValid(commentDto))
                    {
                        sb.AppendLine(ErrorMessage);

                        continue;
                    }

                    var user = context.Users.FirstOrDefault(u => u.Username == commentDto.User);

                    var post = context.Posts.FirstOrDefault(p => p.Id == commentDto.Post.Id);

                    if (user == null || post == null)
                    {
                        sb.AppendLine(ErrorMessage);

                        continue;
                    }

                    var comment = new Comment()
                    {
                        User = user,
                        UserId = user.Id,
                        Post = post,
                        PostId = post.Id,
                        Content = commentDto.Content
                    };

                    commentsToAdd.Add(comment);

                    sb.AppendLine($"Successfully imported Comment {comment.Content}.");
                }

                context.Comments.AddRange(commentsToAdd);

                context.SaveChanges();

                return sb.ToString().Trim();
            }
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}