﻿using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TabloidMVC.Models;
using TabloidMVC.Utils;

namespace TabloidMVC.Repositories
{
    public class CommentRepository : BaseRepository, ICommentRepository
    {
        public CommentRepository(IConfiguration config) : base(config) { }

        // Method to get all comments from specified post 
        public List<Comment> GetCommentsByPost(int postId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT
											c.Id,
											c.Subject,
											c.Content,
											c.CreateDateTime,
											c.PostId,
											c.UserProfileId,
											p.Id AS IdPost,
											p.Title,
											p.Content AS PostContent,
											p.ImageLocation,
											p.CreateDateTime AS PostCreateDateTime,
											p.PublishDateTime,
											p.IsApproved,
											p.CategoryId,
											p.UserProfileId,
											u.Id AS IdUserProfile,
											u.DisplayName,
											u.FirstName,
											u.LastName,
											u.Email,
											u.CreateDateTime AS UserProfileCreateDateTime,
											u.ImageLocation AS UserProfileImageLocation,
											u.UserTypeId
										FROM
											Comment c
											LEFT JOIN Post p ON c.PostId = p.Id
											LEFT JOIN UserProfile u ON c.UserProfileId = u.Id
										WHERE
											c.PostId = @Id
										ORDER BY
											c.CreateDateTime DESC; ";

                    cmd.Parameters.AddWithValue("@Id", postId);

                    var reader = cmd.ExecuteReader();

                    var comments = new List<Comment>();

                    while (reader.Read())
                    {
                        comments.Add(NewCommentFromReader(reader));
                    }

                    reader.Close();

                    return comments;
                }
            }
        }

        // Method to add new comment
        public void AddComment(Comment comment)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Comment
	                                        (PostId, UserProfileId, Subject, Content, CreateDateTime)
                                        OUTPUT INSERTED.Id
                                        VALUES
	                                        (@PostId, @UserProfileId, @Subject, @Content, @CreateDateTime)";

                    cmd.Parameters.AddWithValue("@PostId", comment.PostId);
                    cmd.Parameters.AddWithValue("@UserProfileId", comment.UserProfileId);
                    cmd.Parameters.AddWithValue("@Subject", comment.Subject);
                    cmd.Parameters.AddWithValue("@Content", comment.Content);
                    cmd.Parameters.AddWithValue("@CreateDateTime", comment.CreateDateTime);

                    comment.Id = (int)cmd.ExecuteScalar();
                }
            }
        }

        // Method to get specific comment by id
        public Comment GetCommentById(int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT
	                                        c.Id,
	                                        c.Subject,
	                                        c.Content,
	                                        c.CreateDateTime,
	                                        c.PostId,
	                                        c.UserProfileId,
	                                        p.Id AS IdPost,
	                                        p.Title,
	                                        p.Content AS PostContent,
	                                        p.ImageLocation,
	                                        p.CreateDateTime AS PostCreateDateTime,
	                                        p.PublishDateTime,
	                                        p.IsApproved,
	                                        p.CategoryId,
	                                        p.UserProfileId,
	                                        u.Id AS IdUserProfile,
	                                        u.DisplayName,
	                                        u.FirstName,
	                                        u.LastName,
	                                        u.Email,
	                                        u.CreateDateTime AS UserProfileCreateDateTime,
	                                        u.ImageLocation AS UserProfileImageLocation,
	                                        u.UserTypeId
                                        FROM
	                                        Comment c
	                                        LEFT JOIN Post p ON c.PostId = p.Id
	                                        LEFT JOIN UserProfile u ON c.UserProfileId = u.Id
                                        WHERE
	                                        c.Id = @Id";

                    cmd.Parameters.AddWithValue("@Id", Id);

                    var reader = cmd.ExecuteReader();

                    Comment comment = null;

                    if (reader.Read())
                    {
                        comment = NewCommentFromReader(reader);
                    }

                    reader.Close();

                    return comment;
                }
            }
        }

        // Method deletes specific comment from database
        public void DeleteComment(int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"DELETE FROM Comment
                                        WHERE Id = @Id";

                    cmd.Parameters.AddWithValue("@Id", Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Method updates existing comment in database
        public void UpdateComment(Comment comment)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Comment
                                        SET
	                                        PostId = @PostId,
	                                        UserProfileId = @UserProfileId,
	                                        Subject = @Subject,
	                                        Content = @Content,
	                                        CreateDateTime = @CreateDateTime
                                        WHERE
	                                        Id = @Id";

                    cmd.Parameters.AddWithValue("@PostId", comment.PostId);
                    cmd.Parameters.AddWithValue("@UserProfileId", comment.UserProfileId);
                    cmd.Parameters.AddWithValue("@Subject", comment.Subject);
                    cmd.Parameters.AddWithValue("@Content", comment.Content);
                    cmd.Parameters.AddWithValue("@CreateDateTime", comment.CreateDateTime);
                    cmd.Parameters.AddWithValue("@Id", comment.Id);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Method creates new Comment object with corresponding properties extracting data from reader
        private Comment NewCommentFromReader(SqlDataReader reader)
        {
            return new Comment()
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Subject = reader.GetString(reader.GetOrdinal("Subject")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                CreateDateTime = reader.GetDateTime(reader.GetOrdinal("CreateDateTime")),
                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                UserProfileId = reader.GetInt32(reader.GetOrdinal("UserProfileId")),
                Post = new Post()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("IdPost")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Content = reader.GetString(reader.GetOrdinal("PostContent")),
                    ImageLocation = DbUtils.GetNullableString(reader, "ImageLocation"),
                    CreateDateTime = reader.GetDateTime(reader.GetOrdinal("PostCreateDateTime")),
                    PublishDateTime = DbUtils.GetNullableDateTime(reader, "PublishDateTime"),
                    IsApproved = reader.GetBoolean(reader.GetOrdinal("IsApproved")),
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    UserProfileId = reader.GetInt32(reader.GetOrdinal("UserProfileId"))
                },
                UserProfile = new UserProfile()
                {
                    Id = reader.GetInt32(reader.GetOrdinal("IdUserProfile")),
                    DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    CreateDateTime = reader.GetDateTime(reader.GetOrdinal("UserProfileCreateDateTime")),
                    ImageLocation = DbUtils.GetNullableString(reader, "UserProfileImageLocation"),
                    UserTypeId = reader.GetInt32(reader.GetOrdinal("UserTypeId"))
                }
            };
        }
    }
}
