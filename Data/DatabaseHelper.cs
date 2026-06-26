using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace CybersecurityBotWinForms.Data
{
    // this class handles everything to do with the MySQL database
    // it sets up the table and lets us add, read, update and delete tasks
    public static class DatabaseHelper
    {
        private const string ConnectionString =
            "Server=localhost;Port=3306;Database=cyberguard_db;Uid=root;Pwd=1234;";

        // runs once when the app starts to make sure the database and table exist
        public static void Initialise()
        {
            string setupConn = "Server=localhost;Port=3306;Uid=root;Pwd=1234;";
            using var conn = new MySqlConnection(setupConn);
            conn.Open();

            using (var cmd = new MySqlCommand("CREATE DATABASE IF NOT EXISTS cyberguard_db;", conn))
                cmd.ExecuteNonQuery();

            conn.ChangeDatabase("cyberguard_db");

            string createTable = @"
                CREATE TABLE IF NOT EXISTS tasks (
                    Id          INT AUTO_INCREMENT PRIMARY KEY,
                    Title       VARCHAR(200)  NOT NULL,
                    Description VARCHAR(1000) NOT NULL,
                    ReminderDate DATE          NULL,
                    IsCompleted  TINYINT(1)   NOT NULL DEFAULT 0,
                    CreatedAt   DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP
                );";
            using var tableCmd = new MySqlCommand(createTable, conn);
            tableCmd.ExecuteNonQuery();
        }

        // adds a new task and returns the new row's ID
        public static int AddTask(string title, string description, DateTime? reminderDate)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            string sql = @"INSERT INTO tasks (Title, Description, ReminderDate)
                           VALUES (@title, @desc, @reminder);
                           SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@desc", description);
            cmd.Parameters.AddWithValue("@reminder", (object?)reminderDate ?? DBNull.Value);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // pulls all tasks from the database
        public static List<TaskItem> GetAllTasks()
        {
            var list = new List<TaskItem>();

            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            string sql = "SELECT Id, Title, Description, ReminderDate, IsCompleted, CreatedAt FROM tasks ORDER BY CreatedAt DESC;";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new TaskItem
                {
                    Id = reader.GetInt32("Id"),
                    Title = reader.GetString("Title"),
                    Description = reader.GetString("Description"),
                    ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate"))
                                       ? null
                                       : reader.GetDateTime("ReminderDate"),
                    IsCompleted = reader.GetBoolean("IsCompleted"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return list;
        }

        // marks a task as completed
        public static void MarkCompleted(int id)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand("UPDATE tasks SET IsCompleted = 1 WHERE Id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // removes a task completely
        public static void DeleteTask(int id)
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM tasks WHERE Id = @id;", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    // holds one row from the tasks table
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}