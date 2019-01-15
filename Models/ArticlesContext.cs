using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace philsearch.Models
{
    public class ArticlesContext
    {

        public string ConnectionString { get; set; }
        public string SearchArticlesWS { get; set; }
        public string SemanticNetworkWS { get; set; }

        public ArticlesContext(string connectionString, string searchArticlesWS, string semanticNetworkWS)
        {
            this.ConnectionString = connectionString;
            this.SearchArticlesWS = searchArticlesWS;
            this.SemanticNetworkWS = semanticNetworkWS;
        }

        private SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        public List<Article> GetArticles(string filter)
        {
            List<Article> list = new List<Article>();

            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("select * from articles where art_id in (" + filter + ")", conn);

                using (SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Article()
                            {
                                ArtId = reader["art_id"].ToString(),
                                Title = reader["title"].ToString().Replace("?", ""),
                                Abstract = reader["art_abstract"].ToString(),
                                PubYear = reader["pub_year"].ToString(),
                                PubName = reader["pub_name"].ToString(),
                                PubInfo = reader["pub_info"].ToString(),
                                Url=reader["url"].ToString()

                            });
                        }
                    }
                }
            }

            foreach (Article a in list)
            {
                a.Authors = GetAuthors(a.ArtId);
                a.Categories = GetCategories(a.ArtId);
                a.Features = GetFeatures(a.ArtId);
                a.References = GetReferences(a.ArtId);
            };

            return list;
        }


        public Article GetArticleInfo(string artId)
        {
            Article result = new Article();

            DataSet ds = new DataSet("ArticleInfo");
            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                string query = "";
                query = "Select * from articles Where art_id = '" + artId + "'; ";
                query = query + "Select * from articles_authors Where art_id = '" + artId + "';";
                query = query + "Select * from articles_categories Where art_id = '" + artId + "';";
                query = query + "Select * from articles_features Where art_id = '" + artId + "';";
                query = query + "Select a.art_id, b.* from articles_biblio a inner join biblio b on a.biblio_id = b.biblio_id Where art_id = '" + artId + "';";
                SQLiteCommand cmd = new SQLiteCommand(query, conn);

                using (SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd))
                {   /*
                    SQLiteCommand cmd = new SQLiteCommand("usp_GetArticleInfo", conn);
                    cmd.Parameters.AddWithValue("@artId", artId);
                    cmd.CommandType = CommandType.StoredProcedure;
                    */
                    SQLiteDataAdapter da = new SQLiteDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(ds);

                    result = ReadArticleInfo(ds.Tables[0]);
                    result.Authors = ReadAuthorsInfo(ds.Tables[1]);
                    result.Categories = ReadCategoriesInfo(ds.Tables[2]);
                    result.Features = ReadFeaturesInfo(ds.Tables[3]);
                    result.References = ReadReferencesInfo(ds.Tables[4]);
                }
            }
            return result;
        }


        private Article ReadArticleInfo(DataTable dt)
        {
            Article result = new Article();
            foreach(DataRow row in dt.Rows)
            {
                result.ArtId = row["art_id"].ToString();
                result.Title = row["title"].ToString().Replace("?", "");
                result.Abstract = row["art_abstract"].ToString();
                result.PubYear = row["pub_year"].ToString();
                result.PubName = row["pub_name"].ToString();
                result.PubInfo = row["pub_info"].ToString();
                result.Url = row["url"].ToString();
            };

            return result;            
            
        }

        private List<String> ReadAuthorsInfo(DataTable dt)
        {
            List<String> result = new List<String>();

            foreach (DataRow row in dt.Rows)
            {
                result.Add(row["author"].ToString());
            };

            return result;
        }

        private List<String> ReadCategoriesInfo(DataTable dt)
        {
            List<String> result = new List<String>();

            foreach (DataRow row in dt.Rows)
            {
                result.Add(row["category"].ToString());
            };

            return result;
        }

        private List<Article.Feature> ReadFeaturesInfo(DataTable dt)
        {
            List<Article.Feature> result = new List<Article.Feature>();

            foreach (DataRow row in dt.Rows)
            {
                result.Add(new Article.Feature()
                {
                    Id = row["feature"].ToString(),
                    TfIdf = Convert.ToDouble(row["tfidf"])
                });
            };

            return result;
        }

        private List<Article.Reference> ReadReferencesInfo(DataTable dt)
        {
            List<Article.Reference> result = new List<Article.Reference>();

            foreach (DataRow row in dt.Rows)
            {
                result.Add(new Article.Reference()
                {
                    Id = row["biblio_id"].ToString(),
                    Year = row["year"].ToString(),
                    Author = row["author"].ToString(),
                    Title = row["title"].ToString().Replace("?", ""),
                    Journal = row["journal"].ToString().Replace("?", ""),
                    Number = row["number"].ToString(),
                    Volume = row["volume"].ToString(),
                    Pages = row["pages"].ToString(),
                    Publisher = row["publisher"].ToString().Replace("?", "")

                });                    
            }

            return result;
        }

        private List<String> GetAuthors(string artId)
        {
            List<String> list = new List<String>() { };

            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("select * from articles_authors where art_id = '" + artId + "'", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["author"].ToString());
                    }
                }
            }
            return list;
        }

        private List<String> GetCategories(string artId)
        {
            List<String> list = new List<String>();

            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("select * from articles_categories where art_id = '" + artId + "'", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader["category"].ToString());
                    }
                }
            }
            return list;
        }

        private List<Article.Reference> GetReferences(string artId)
        {
            List<Article.Reference> list = new List<Article.Reference>();

            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("Select b.* from articles_biblio a inner join biblio b on a.biblio_id = b.biblio_id  where art_id = '" + artId + "'", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Article.Reference()
                        {
                            Id = reader["biblio_id"].ToString(),
                            Year = reader["year"].ToString(),
                            Author = reader["author"].ToString(),
                            Title = reader["title"].ToString().Replace("?",""),
                            Journal = reader["journal"].ToString().Replace("?", ""),
                            Number = reader["number"].ToString(),
                            Volume = reader["volume"].ToString(),
                            Pages = reader["pages"].ToString(),
                            Publisher = reader["publisher"].ToString().Replace("?", "")

                        });
                    }
                }
            }
            return list;
        }



        private List<Article.Feature> GetFeatures(string artId)
        {
            List<Article.Feature> list = new List<Article.Feature>();

            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand("select * from articles_features where art_id = '" + artId + "'", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Article.Feature()
                        {
                            Id = reader["feature"].ToString(),
                            TfIdf = Convert.ToDouble(reader["tfidf"])
                        });
                    }
                }
            }
            return list;
        }

    }
}
