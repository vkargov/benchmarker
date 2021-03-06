﻿using System;
using System.Collections.Generic;
using Npgsql;
using System.Linq;

namespace Benchmarker.Models
{
	public class Commit
	{
		public string Hash { get; set; }
		public Product Product { get; set; }
		public string Branch { get; set; }
		public string MergeBaseHash { get; set; }
		public DateTime? CommitDate { get; set; }
		
		public Commit ()
		{
		}

		bool ExistsInPostgres (NpgsqlConnection conn)
		{
			var parameters = new PostgresRow ();
			parameters.Set ("hash", NpgsqlTypes.NpgsqlDbType.Varchar, Hash);
			parameters.Set ("product", NpgsqlTypes.NpgsqlDbType.Varchar, Product.Name);
			return PostgresInterface.Select (conn, "commit", new string[] { "commitDate" }, "hash = :hash and product = :product", parameters).Count () > 0;
		}

		public string GetOrUploadToPostgres (NpgsqlConnection conn)
		{
			if (ExistsInPostgres (conn))
				return Hash;

			Logging.GetLogging ().Info ("commit " + Hash + " for " + Product + " not found - inserting");

			var row = new PostgresRow ();
			row.Set ("hash", NpgsqlTypes.NpgsqlDbType.Varchar, Hash);
			row.Set ("product", NpgsqlTypes.NpgsqlDbType.Varchar, Product.Name);
			row.Set ("commitDate", NpgsqlTypes.NpgsqlDbType.TimestampTZ, CommitDate);
			row.Set ("branch", NpgsqlTypes.NpgsqlDbType.Varchar, Branch);
			row.Set ("mergeBaseHash", NpgsqlTypes.NpgsqlDbType.Varchar, MergeBaseHash);
			return PostgresInterface.Insert<string> (conn, "commit", row, "hash");
		}
	}
}
