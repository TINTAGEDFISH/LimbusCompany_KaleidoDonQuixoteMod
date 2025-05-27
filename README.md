# LimbusCompany_KaleidoDonQuixoteMod
A mod that causes LCB DonQuixote to shift into a random DonQuixote personality at the start of each turn.  
这是一个让初始唐在每回合开始变成随机小唐的边狱巴士私服模组

### 注： 
### 1.此模组仅能在边狱巴士私人服务器使用 
### 2.此安装教程默认你已经成功配置私人服务器环境，本教程不会去另外花篇幅去讲述私服资源的获取与安装 


### 安装教程： 

1.使用集成开发环境创建C#类库(.dll)项目.NET Framework项目，然后把本仓库的代码复制进去   

2.引入项目依赖，即私服环境配置中由BepInEx生成的动态链接库，例如game\BepInEx\core，game\BepInEx\interop，game\BepInEx\unity-libs这些目录下的文件   

3.生成.dll文件，然后将其放置在game\BepInEx\plugins目录下   

4.将KaleidoDonQuixoteMod文件夹丢入私服的模组文件夹下（当然这一步不是必须的，哪怕你自己写一个人格也行，只要保证技能列表内包含所有小唐可能会使用的技能就行）
