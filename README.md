# Simple.Data
A lightweight, dynamic data access component for .NET, written in C#.
## What is it?
Prompted by the need for an easy-to-use database access component which prevents SQL injection attacks while not requiring lots of boilerplate ADO.NET code or a pre-generated ORM model. Inspired by Ruby's ActiveRecord and DataMapper gems.

Instead of

    public User FindUserByEmail(string email)
	{
		User user = null;
        using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
	    using (var command = new SqlCommand("select id, email, hashed_password, salt from users where email = @email", connection))
	    {
	        command.Parameters.Add("@email", SqlDbType.NVarChar, 50).Value = form.Email);
		    connection.Open();
		    using (var reader = command.ExecuteReader())
		    {
			    if (reader.Read())
			    {
				    user = new User {Id = reader.GetInt32(0), Email = reader.GetString(1), Password = reader.GetString(2), salt = reader.GetString(3)};
				}
			}
		}
		return user;
	}

why not just write

	public User FindUserByEmail(string email)
	{
		return Database.Open().Users.FindByEmail(email);
	}

and take the rest of the morning off?

Simple.Data does this by using the dynamic features of .NET 4 to interpret method and property names at runtime and map them to your underlying data-store with a convention-based approach.
For the code above, there is no pre-defined type with a Users property, and no FindByEmail method. Within Simple.Data, that single line of code is converted into all the ADO.NET
boilerplate for you.

## Multiple database and NoSQL store support
Because Simple.Data provides a sort of dynamic Domain Specific Language to represent queries, inserts, updates and deletes, it is able to support not only a wide range of RDBMS
engines, but also non-SQL-based data stores such as MongoDB. It has an open and flexible model of Adapters and Providers which make it simple to write plug-ins to map to
almost any back-end.

Currently, Simple.Data has adapters for:

* ADO-based access to relational databases, with providers for:
	* SQL Server 2005 and later (including SQL Azure)
	* SQL Server Compact Edition 4.0
        * Oracle
	* MySQL 4.0 and later
	* SQLite
        * PostgreSQL
        * SQLAnywhere
        * Informix
* MongoDB
* OData

Work is in progress to support Azure Table Storage. I'm also ensuring that Simple.Data works on Mono by the 1.0 release.

If you'd like to create an adapter or provider and need some help to get started, drop in on the mailing list (see below).

## Resources
* Simple.Data can be installed from [NuGet](http://nuget.org/)
* Find more information in [the wiki](https://github.com/markrendle/Simple.Data/wiki)
* [Documentation!](http://simplefx.org/simpledata/docs/)
* Ask questions or report issues on [the mailing list](http://groups.google.com/group/simpledata)
* Follow [@markrendle on Twitter](http://twitter.com/markrendle) for updates
* Check out [my blog](http://blog.markrendle.net/) for the latest news

<a href='http://www.pledgie.com/campaigns/15965'><img alt='Click here to lend your support to: Simple.Data and make a donation at www.pledgie.com !' src='http://www.pledgie.com/campaigns/15965.png?skin_name=chrome' border='0' /></a>
