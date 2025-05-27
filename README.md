# LimbusCompany_KaleidoDonQuixoteMod
A mod that causes LCB DonQuixote to shift into a random DonQuixote personality at the start of each turn.  
这是一个让初始唐在每回合开始变成随机小唐的边狱巴士私服模组



### Important Notes:
### 1. This mod is exclusively compatible with the LimbusCompany private server.
### 2. This guide assumes you have already configured the private server environment. Resource acquisition/installation for private servers will not be covered here.

### Installation Guide:

1.Create a C# Class Library (.dll) project in your IDE using .NET Framework, then clone/copy this repository's code into the project.

2.Add dependencies: Reference the BepInEx-generated DLLs from your private server’s environment, specifically:
game/BepInEx/core
game/BepInEx/interop
game/BepInEx/unity-libs

3.Build the .dll and place it in game/BepInEx/plugins.

4.Optional: Drop the KaleidoDonQuixoteMod folder into the private server’s mod directory.
(This step is only required if using pre-built personalities. Custom personalities are acceptable provided all skill IDs used by "Little Don" are included in your skill pool.)

### 注： 
### 1.此模组仅能在边狱巴士私人服务器使用 
### 2.此安装教程默认你已经成功配置私人服务器环境，本教程不会去另外花篇幅去讲述私服资源的获取与安装 


### 安装教程： 

1.使用集成开发环境创建C#类库(.dll)项目.NET Framework项目，然后把本仓库的代码复制进去   

2.引入项目依赖，即私服环境配置中由BepInEx生成的动态链接库，例如game\BepInEx\core，game\BepInEx\interop，game\BepInEx\unity-libs这些目录下的文件   

3.生成.dll文件，然后将其放置在game\BepInEx\plugins目录下   

4.将KaleidoDonQuixoteMod文件夹丢入私服的模组文件夹下（当然这一步不是必须的，哪怕你自己写一个人格也行，只要保证技能列表内包含所有小唐可能会使用的技能就行）
