// Run in Node.JS
// node generateSearchRegex

// https://stackoverflow.com/questions/7744912/making-a-javascript-string-sql-friendly
function mysql_escape(str) {
    return str.replace(/[\0\x08\x09\x1a\n\r"'\\\%]/g, function (char) {
        switch (char) {
            case "\0":
                return "\\0";
            case "\x08":
                return "\\b";
            case "\x09":
                return "\\t";
            case "\x1a":
                return "\\z";
            case "\n":
                return "\\n";
            case "\r":
                return "\\r";
            case "\"":
            case "'":
            case "\\":
            case "%":
                return "\\" + char; // prepends a backslash to backslash, percent,
            // and double/single quotes
            default:
                return char;
        }
    });
}
function cs_verbatim_escape(str) {
    return `@"${str.replaceAll('"', '""')}"`;
}

const replacements = [
    ["\\W", "  "],
    [" [a-zA-Z]([a-z]*) ", "  "],
    ["([A-Z]{2,})([a-z]+)", "$0 $1 $2"],
    ["([a-zA-Z][A-Z]*)([A-Z]| )", "$1  $2"],
    ["([a-zA-Z][a-z]+)", "$1  "],
    ["_", "  "],
    ["([a-zA-Z]([A-Z][A-Z]*|[a-z][a-z]*)|[0-9]+)", "$1 "],
    [" +", " "],
    ["^( ?)(.*)( +)$", "$2"]
];
console.log(`
MySQL:

    ALTER TABLE WikiCommits MODIFY COLUMN Words VARCHAR(5000) GENERATED ALWAYS AS ${generateMySQL()} STORED;
`);
function generateMySQL ()
{
    let sql = 'CONCAT(" ", Markdown, " ")';
    for (let [start, end] of replacements)
        sql = `REGEXP_REPLACE(${sql}, "${mysql_escape(start)}", "${mysql_escape(end)}", 1, 0, "c")`;
    return sql;
}
console.log(`C#:

new List<(Regex start, string end)>
{
${replacements.map(([start, end]) => `    (new Regex(${cs_verbatim_escape(start)}), ${cs_verbatim_escape(end)})`).join(',\n')}
}`);