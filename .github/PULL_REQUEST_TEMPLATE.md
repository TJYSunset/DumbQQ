本项目的NuGet包通过AppVeyor在build时发布，但是没有让NuGet的版本号与CI的build号自动同步。如果这个PR有对项目文件的修改，请修改`AssemblyInfo.cs`里的版本号。

PR的版本号递增规则：小的更改增加生成号（第三个数字），较大更改增加次版本号（第二个数字）。