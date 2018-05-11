# 验证Linux ELF文件的作用、格式

[TOC]

注：ELF文件共有三种，下面依次分析

## 重定位文件

### 获取

* 程序源码

![](http://p5bpzbkxa.bkt.clouddn.com/c%E8%AF%AD%E8%A8%80%E7%A8%8B%E5%BA%8F%E6%BA%90%E7%A0%81.png)



* 编译获取.o文件并查看格式

![](http://p5bpzbkxa.bkt.clouddn.com/%E7%BC%96%E8%AF%91%E5%B9%B6%E6%9F%A5%E7%9C%8B.o%E6%96%87%E4%BB%B6%E6%A0%BC%E5%BC%8F.png)

可以看出，.o文件符合ELF文件格式

### 格式分析

#### ELF header

![](http://p5bpzbkxa.bkt.clouddn.com/ELF_header.png)

从Magic可以得知，该.o文件为64位目标高位在前版本号为01

目标文件版本为当前版本

从type可以看出该.o文件为relocatable object file，即可重定向文件，对于一个REL文件而言，它是不会被加载运行的，因此入口点地址为0，同理，.o文件中也不存在program header，因此Start of program headers、Number of program headers和Size of program headers都等于0.

此header的大小为64byte

从.o文件的1240偏移处是section header table，其中有13个section header，每个section header entry 占用了64byte，总共占用了13×64=832个byte

节区头部表格中与节区名称字符串表相关的表项共有10项

#### Section header table

![](http://p5bpzbkxa.bkt.clouddn.com/%E9%87%8D%E5%AE%9A%E5%90%91%E6%96%87%E4%BB%B6section%20header%20table.png)

正如ELF header中所描述的那样，section header table 中有13个entry，第一个section是inactive的section

由于.o文件不具备运行的条件，因此所有section的地址都为0

flags为A说明该节区在执行过程中占用内存，正文段、数据段显然占用内存，因此有A标记，符号表、字符串表、重定位信息等section就没有A标记，因为他们和程序执行无关，主要是向linker提供后续链接所需要的信息

.text是程序代码，PROGBITS说明该secton包含了程序定义的信息。占据.o文件便宜0x40处，整个长度为0x91，没有对齐约束

.data的size是4，初始化的全局变量OS_work_data就位于此处

.bbs处放置了未初始化的全局变量OS_work_bbs，所以在.o文件中没有它的位置，.bbs的size为0

.rodata对应常量字符串，size为十六进制的39即十进制的57，对齐方式为1说明存储的常量字符串的长度恰好为57，字符串“my_name"长度为8，print_f函数中printf内的常量字符串长度为28，main中的printf内的常量字符串的长度为21，总长度为57，由此可见，字符串常量包括所有函数中的所有字符串

在所有的section中，.rela.text .comment .rela.eh_frame .symtab是有固定size的item组成的，bss section被标记了NOBITS，这个标记含义和PROGBITS一样，只不过它不占用.o 文件的size

数据段（.data .bbs）都有W标记，说明均是可写的

#### Program header table

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8F%AF%E9%87%8D%E5%AE%9A%E5%90%91%E6%96%87%E4%BB%B6%E4%B8%AD%E6%B2%A1%E6%9C%89%E7%A8%8B%E5%BA%8F%E5%A4%B4.png)

####Symbol Table

![](http://p5bpzbkxa.bkt.clouddn.com/%E9%87%8D%E5%AE%9A%E5%90%91%E6%96%87%E4%BB%B6section%20table%20header.png)

符号表信息：

![](http://p5bpzbkxa.bkt.clouddn.com/%E9%87%8D%E5%AE%9A%E4%BD%8D%E6%96%87%E4%BB%B6%E7%AC%A6%E5%8F%B7%E8%A1%A8%E4%BF%A1%E6%81%AF.png)

OS_work_data与OS_work_bbs是数据，Type为OBJECT，size为4byte

print_f、sum、main为函数，Type为FUNC，长度不等

printf是c语言标准库里的函数，类型为NOTYPE，Ndx为UND，意味着符号没有定义，当链接编辑器将此目标文件与其他定义了该符号的目标文件进行组合时,此文件中对该符号的引用将被链接到实际定义的位置。

OS_work_bbs的Ndx为COM，标注了一个尚未分配的公共块。符号的取值给出了对齐约束,与节区的 sh_addralign成员类似。就是说,链接编辑器将为符号分配存储空间,地址位于 st_value 的倍数处。符号的大小给出了所需要的字节数。

ABS表示该符号在后续的relocation中不会被更改

type是SECTION的那些项次都是和section相关的符号定义，主要是用在relocation的时候

#### 重定位表

![](http://p5bpzbkxa.bkt.clouddn.com/%E9%87%8D%E5%AE%9A%E4%BD%8D%E8%A1%A8.png)

## 可执行文件

### 获取

![](http://p5bpzbkxa.bkt.clouddn.com/%E7%94%9F%E6%88%90%E5%8F%AF%E6%89%A7%E8%A1%8C%E6%96%87%E4%BB%B6.png)

可以看出，OS_work为可执行文件

### 分析

#### ELF header

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8F%AF%E6%89%A7%E8%A1%8C%E6%96%87%E4%BB%B6ELF%20header.png)

从Magic可以得知，该可执行文件为64位目标高位在前版本号为01

目标文件版本为当前版本

从type可以看出该文件为可执行文件

入口点地址为0x400430，Start of program headers为64byte、Number of program headers为9，Size of program headers为56.

此header的大小为64byte

从可执行文件的6760偏移处是section header table，其中有31个section header，每个section header entry 占用了64byte，总共占用了31×64=1984个byte

节区头部表格中与节区名称字符串表相关的表项共有28项

#### Section header table

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8F%AF%E6%89%A7%E8%A1%8C%E6%96%87%E4%BB%B6%E8%8A%82%E5%A4%B41.png)

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8F%AF%E6%89%A7%E8%A1%8C%E6%96%87%E4%BB%B6%E8%8A%82%E5%A4%B42.png)

 可以看出，新出现了几个section，.plt与.plt.got，前者是过程链接表，后者是全局偏移表

#### PLT

![](http://p5bpzbkxa.bkt.clouddn.com/PLT.png)

<printf@plt>中，400406地址恰好是一条push指令，随后是一条jmp指令，执行完push指令之后，就会跳转到4003f0地址处

其它指令同理

#### GOT

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8F%AF%E6%89%A7%E8%A1%8C%E6%96%87%E4%BB%B6GOT.png)

Node中显示重定位信息还没有被应用到此处，所以无法从这里获取到更多信息

#### 重定位表

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8F%AF%E6%89%A7%E8%A1%8C%E6%96%87%E4%BB%B6%E9%87%8D%E5%AE%9A%E4%BD%8D%E8%A1%A8.png)

注意到Info一列的1、2、 3分别与下面Num为1 、2 、3的内容对应，其中1号包含了和printf符号相关的各种信息，也就是说，在执行过程链接表中的第一项指令jmpq调用动态链接器后，动态链接器因为有了push$0x0，从而可以通过重定位表的r_info找到对应符号printf在符号表.dynsym中的相关信息。

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%AF%B9%E5%BA%94.png)

符号表中还有Offset和Type两个重要信息，前者表示该重定位操作后可能影响的地址，后者表示修改地址的方式。

#### Program header table

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8F%AF%E6%89%A7%E8%A1%8C%E6%96%87%E4%BB%B6%E7%A8%8B%E5%BA%8F%E5%A4%B4.png)

#### Segment

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8F%AF%E6%89%A7%E8%A1%8C%E6%96%87%E4%BB%B6segment.png)

#### 动态节区

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8F%AF%E6%89%A7%E8%A1%8C%E6%96%87%E4%BB%B6%E5%8A%A8%E6%80%81%E8%8A%82%E5%8C%BA.png)

 ## 动态库文件

### 获取

![](http://p5bpzbkxa.bkt.clouddn.com/%E7%94%9F%E6%88%90%E5%8A%A8%E6%80%81%E9%93%BE%E6%8E%A5%E5%BA%93%E6%97%B6%E5%8F%91%E7%94%9F%E9%94%99%E8%AF%AF.png)

解决方法：添加-fPIC后重新编译.c文件：

![](http://p5bpzbkxa.bkt.clouddn.com/%E7%94%9F%E6%88%90%E5%85%B1%E4%BA%AB%E7%9B%AE%E6%A0%87%E6%88%91%E9%82%A3%E4%BB%B6.png)

### 分析

#### ELF header

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8A%A8%E6%80%81%E9%93%BE%E6%8E%A5%E5%BA%93%E6%96%87%E4%BB%B6ELF%20header.png)

#### Program header table

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8A%A8%E6%80%81%E5%BA%93%E6%96%87%E4%BB%B6%E7%A8%8B%E5%BA%8F%E5%A4%B4.png)

#### Segment

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8A%A8%E6%80%81%E5%BA%93%E6%96%87%E4%BB%B6segment.png)

#### 重定位表

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8A%A8%E6%80%81%E5%BA%93%E6%96%87%E4%BB%B6%E9%87%8D%E5%AE%9A%E4%BD%8D%E8%A1%A8.png)

#### Section

![](http://p5bpzbkxa.bkt.clouddn.com/%E5%8A%A8%E6%80%81%E5%BA%93%E6%96%87%E4%BB%B6section.png)



