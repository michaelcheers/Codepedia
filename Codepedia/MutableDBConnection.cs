using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codepedia
{
    public class MutableDBConnection : IDisposable
    {
        //const string Host = "127.0.0.1", Port = "3306", DBName = "Codepedia", Username = "root", Password = "mypass123";
        public MySqlConnection conn = new("server=127.0.0.1;uid=root;pwd=mypass123;database=Codepedia");//$"Server={Host}; database={DBName}; UID={Username}; password={Password}; port={Port};SSL Mode=none");

        MutableDBConnection() { }

        public static async Task<MutableDBConnection> Create()
        {
            MutableDBConnection msdc = new();
            await msdc.conn.OpenAsync();
            await new CommandCreator(msdc, "SET time_zone = '+0:00'").Command.ExecuteNonQueryAsync();
            return msdc;
        }

        public void Dispose() => conn.Dispose();

        public Task<MutableDBTransaction> CreateTransaction() => MutableDBTransaction.Create(this);
    }


    public class MutableDBTransaction : IDisposable
    {
        MySqlTransaction trans;
        public MutableDBConnection connO;
        MySqlConnection conn => connO.conn;
        bool commited = false;

        MutableDBTransaction() { }

        public static async Task<MutableDBTransaction> Create(MutableDBConnection conn) => new()
        {
            connO = conn,
            trans = await conn.conn.BeginTransactionAsync()
        };

        public void Commit()
        {
            trans.Commit();
            commited = true;
            Dispose();
        }

        public void Dispose() { if (!commited) trans.Rollback(); trans.Dispose(); }
    }

    public class Assigner<T>
    {
        public Assigner(T v) => Value = v;
        public T Value { get; }
    }

    public class CommandCreator
    {
        public CommandCreator(MySqlConnection conn) => Command = conn.CreateCommand();
        public CommandCreator(MutableDBConnection mdc) : this(mdc.conn) { }
        public CommandCreator(MutableDBConnection mdc, string commandText) : this(mdc.conn)
            => CommandText = commandText;
        public CommandCreator(string commandText, MutableDBConnection mdc) : this(mdc, commandText) { }
        public CommandCreator(string commandText, MutableDBTransaction mdc) : this(mdc.connO, commandText) { }
        public CommandCreator(MutableDBTransaction mdc, string commandText) : this(commandText, mdc) { }

        public CommandCreator(string commandText, MutableDBConnection mdc, Dictionary<string, object> d) : this(mdc, commandText)
        { if (d != null) foreach (var (k, o) in d) Add(k, o); }

        public object this[string parameterName] { set => Add(parameterName, value); }
        public void Add(string parameterName, object value)
        {
            if (parameterName[0] != '@') parameterName = '@' + parameterName;
            Command.Parameters.AddWithValue(parameterName, value);
        }

        public string CommandText { set => Command.CommandText = value; }
        public MySqlCommand Command;

        public int Run0TO2()
        {
            int exec = Command.ExecuteNonQuery();
            if (exec is not (0 or 1 or 2)) throw new Exception($"Non-query returned {exec} rows!");
            return exec;
        }

        public int Run0OR1 ()
        {
            int exec = Command.ExecuteNonQuery();
            if (exec is not (0 or 1)) throw new Exception($"Non-query returned {exec} rows!");
            return exec;
        }

        public void Run1()
        {
            int exec = Command.ExecuteNonQuery();
            if (exec is not 1) throw new Exception($"Non-query returned {exec} rows!");
        }

        public int RunID()
        {
            int exec = Command.ExecuteNonQuery();
            if (exec is not 1) throw new Exception($"Non-query returned {exec} rows!");
            return (int)Command.LastInsertedId;
        }

        public IEnumerable<MySqlDataReader> Run()
        {
            using (MySqlDataReader reader = Command.ExecuteReader())
                while (reader.Read()) yield return reader;
        }
    }
}
