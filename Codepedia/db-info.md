# Initialize MySQL and PHPMyAdmin
(a slight modification of https://migueldoctor.medium.com/run-mysql-phpmyadmin-locally-in-3-steps-using-docker-74eb735fa1fc)
```powershell
docker run --name my-own-mysql -p 3306:3306 -e MYSQL_ROOT_PASSWORD=mypass123 -d mysql:8.0 # mysql:8.0.28
docker run --name my-own-phpmyadmin -d --link my-own-mysql:db -p 8081:80 phpmyadmin/phpmyadmin
docker exec -it my-own-mysql bash
/# mysql --user=root --password=mypass123
```

# Scaffold DB
(make sure it is not running)
```powershell
cd source/repos/Codepedia/Codepedia
dotnet ef dbcontext scaffold "server=127.0.0.1;uid=root;pwd=mypass123;database=Codepedia" MySql.EntityFrameworkCore -o DB -f
```

(source: https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core-scaffold-example.html)

# The Words Column
Adding it:
```sql
ALTER TABLE `WikiCommits` ADD `Words` VARCHAR(5000) GENERATED ALWAYS AS {formula} STORED AFTER `Markdown`, ADD FULLTEXT (`Words`);
```
Changing the formula:
```sql
ALTER TABLE `WikiCommits` MODIFY COLUMN `Words` VARCHAR(5000) GENERATED ALWAYS AS {formula} STORED;
```
`{formula}` =
```sql
(REGEXP_REPLACE(REGEXP_REPLACE(REGEXP_REPLACE(REGEXP_REPLACE(REGEXP_REPLACE(REGEXP_REPLACE(REGEXP_REPLACE(REGEXP_REPLACE(REGEXP_REPLACE(CONCAT(" ", Markdown, " "), "\\W", "  ", 1, 0, "c"), " [a-zA-Z]([a-z]*) ", "  ", 1, 0, "c"), "([A-Z]{2,})([a-z]+)", "$0 $1 $2", 1, 0, "c"), "([a-zA-Z][A-Z]*)([A-Z]| )", "$1  $2", 1, 0, "c"), "([a-zA-Z][a-z]+)", "$1  ", 1, 0, "c"), "_", "  ", 1, 0, "c"), "([a-zA-Z]([A-Z][A-Z]*|[a-z][a-z]*)|[0-9]+)", "$1 ", 1, 0, "c"), " +", " ", 1, 0, "c"), "^( ?)(.*)( +)$", "$2", 1, 0, "c"))
```

# `Users`.`DisplayName` Column
Adding it:
```sql
ALTER TABLE `Users` ADD `DisplayName` VARCHAR(130) GENERATED ALWAYS AS (IF(Role='Admin', CONCAT("♦ ", Username), Username)) STORED AFTER `Username`;
```