using System.Text.RegularExpressions;

var (oldCompany, oldProject) = GetCompanyAndProject(true);
var (company, project) = GetCompanyAndProject(false);
bool sure = YesOrNo();

var oldPrefix = string.Concat(oldCompany, ".", oldProject);
var oldStartName = oldCompany + oldProject;
var prefix = string.Concat(company, ".", project);
var startName = company + project;

var path = Path.Combine("./backend");
var findFileSuffix = new string[] { ".DotSettings", ".cs", ".sln", ".config", ".csproj" };
var findFileName = new string[] { "launchSettings.json" };
Rename(path);
Console.WriteLine("按任意键结束");
Console.ReadKey();

(string, string) GetCompanyAndProject(bool isOld = false)
{
    var oldOrNew = isOld ? "旧" : "新";
    Console.WriteLine($"请输入{oldOrNew}公司名称：");
    var company = Console.ReadLine();
    if (!IsValidString(company))
    {
        throw new ArgumentException("字符串格式错误");
    }
    Console.WriteLine($"请输入{oldOrNew}项目名称：");
    var project = Console.ReadLine();
    if (!IsValidString(project))
    {
        throw new ArgumentException("字符串格式错误");
    }
    return (company!, project!);
}

bool YesOrNo()
{
    Console.WriteLine($"确定重命名吗（yes/no）？");
    var yesOrNo = Console.ReadLine();
    if (string.IsNullOrEmpty(yesOrNo))
    {
        return YesOrNo();
    }
    var input = yesOrNo.ToLower();
    if (input == "yes" || input == "y")
    {
        return true;
    }
    return false;
}

static bool IsValidString(string? input)
{
    if (string.IsNullOrWhiteSpace(input)) return false;
    // 正则表达式：以英文字母开头，后面可以跟英文字母、数字或下划线，但不能以点号结尾
    string pattern = @"^[a-zA-Z][a-zA-Z0-9_]*[^.]$";
    return Regex.IsMatch(input, pattern);
}

static void PrintExecuteInfo(string oldName, string newName)
{
    Console.WriteLine("{0,-20} => {1}", oldName, newName);
}

void Rename(string path)
{
    if (!Directory.Exists(path)) return;
    var files = Directory.GetFiles(path);
    foreach (var item in files)
    {
        var fileName = Path.GetFileName(item);
        var fileSuffix = Path.GetExtension(item);
        var fileDir = Path.GetDirectoryName(item);
        if (!(findFileSuffix.Contains(fileSuffix) || findFileName.Contains(fileName))) continue;
        //重命名内容
        string content = File.ReadAllText(item);
        // 替换字符串
        var newContent = content.Replace(oldStartName, startName).Replace(oldPrefix, prefix);
        // 将新内容写回文件
        File.WriteAllText(item, newContent);
        if (!content.Equals(newContent)) Console.WriteLine("{0}已更新", item);
        //重命名文件名
        if (fileName.StartsWith(oldStartName))
        {
            var newFileName = fileName.Replace(oldStartName, startName);
            var newFilePath = Path.Combine(fileDir!, newFileName);
            File.Move(item, newFilePath);
            PrintExecuteInfo(item, newFilePath);
        }
        if (fileName.StartsWith(oldPrefix))
        {
            var newFileName = fileName.Replace(oldPrefix, prefix);
            var newFilePath = Path.Combine(fileDir!, newFileName);
            File.Move(item, newFilePath);
            PrintExecuteInfo(item, newFilePath);
        }
    }
    var dirs = Directory.GetDirectories(path);
    foreach (var item in dirs)
    {
        var lastDir = Path.GetFileName(item);
        if (lastDir == ".vs") continue;
        var newDirPath = item;
        if (lastDir.StartsWith(oldStartName))
        {
            var newDir = lastDir.Replace(oldStartName, startName);
            newDirPath = Path.Combine(item.Replace(lastDir, ""), newDir);
            Directory.Move(item, newDirPath);
            PrintExecuteInfo(item, newDirPath);
        }
        else if (lastDir.StartsWith(oldPrefix))
        {
            var newDir = lastDir.Replace(oldPrefix, prefix);
            newDirPath = Path.Combine(item.Replace(lastDir, ""), newDir);
            Directory.Move(item, newDirPath);
            PrintExecuteInfo(item, newDirPath);
        }
        Rename(newDirPath);
    }
    var newDirs = Directory.GetDirectories(path);
    //获取差集
    newDirs = newDirs.Except(newDirs).ToArray();
    foreach (var item in newDirs)
    {
        Rename(item);
    }
}