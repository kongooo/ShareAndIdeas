[TOC]

# Linux ELF文件

## 作用

用于定义不同类型的对象文件(Object files)中都放了什么东西、以及都以什么样的格式去放这些东西 

## 类型

* **可重定位的对象文件**：由汇编器汇编生成的.o文件
* **可执行的对象文件**：可执行应用程序
* **可被共享的对象文件**：动态库文件，即.so文件

## 格式

|      链接视图      |      执行视图      |
| :----------------: | :----------------: |
|      ELF头部       |      ELF头部       |
| 程序头部表（可选） |     程序头部表     |
|       节区1        |        段1         |
|         ……         |                    |
|       节区n        |        段2         |
|         ……         |                    |
|         ……         |         ……         |
|     节区头部表     | 节区头部表（可选） |
|                    |                    |

注：

* **除了ELF头部表之外，其它节区和段都没有规定的顺序**
* 右半图是以程序执行视图来看的，编译器在生成目标文件时，通常使用从0开始的相对地址，而在链接过程中，链接器从一个指定的地址开始，根据输入目标文件的顺序，以段（segment）为单位将它们拼装起来。其中每个段可以包括很多个节 

### **ELF header**

描述整个文件的组织，节区部分包含链接视图的大量信息：指令、数据、符号表、重定位信息等

```c
typedef uint32_t        Elf32_Addr;//无符号程序地址
typedef uint16_t        Elf32_Half;//无符号中等大小整数
typedef uint32_t        Elf32_Off;//无符号文件偏移
typedef int32_t         Elf32_Sword;//有符号大整数
typedef uint32_t        Elf32_Word;//无符号大整数

#define EI_NIDENT       16      //!< Size of e_ident[].

typedef struct
{
unsigned char e_ident[EI_NIDENT];     /* Magic number and other info */
Elf64_Half    e_type;                 /* Object file type */
Elf64_Half    e_machine;              /* Architecture */
Elf64_Word    e_version;              /* Object file version */
Elf64_Addr    e_entry;                /* Entry point virtual address */
Elf64_Off     e_phoff;                /* Program header table file offset */
Elf64_Off     e_shoff;                /* Section header table file offset */
Elf64_Word    e_flags;                /* Processor-specific flags */
Elf64_Half    e_ehsize;               /* ELF header size in bytes */
Elf64_Half    e_phentsize;            /* Program header table entry size */
Elf64_Half    e_phnum;                /* Program header table entry count */
Elf64_Half    e_shentsize;            /* Section header table entry size */
Elf64_Half    e_shnum;                /* Section header table entry count */
Elf64_Half    e_shstrndx;             /* Section header string table index */
} Elf64_Ehdr;
```

* ident：给出如何解释文件的提示信息，这些信息独立于处理器，也独立于文件中的其余内容，包含用一表示ELF文件的字符，以及其他一些与机器无关的信息，开头的4个字节值固定不变，为0x7f和ELF三个字符，标志此文件是一个ELF文件
* entry：程序的入口虚拟地址，如果目标文件没有程序入口，可以为0
* flags：保存与文件相关的，特定于处理器的标志。标志名称采用 EF_machine_flag的格式
* shstrndx：节区头部表格中与节区名称字符串表相关的表项的索引。如果文件没有节区名称字符串表，此参数可以为 SHN_UNDEF。 

### **Section header table**

节区表是用来描述各节区的，包括各节区的名字、大小、类型、虚拟内存中的位置、相对文件头的位置等，所有节区都可通过节区表描述，这样连接器就可以根据文件头部表和节区表的描述信息对各种输入的可重定位文件进行合适的链接，包括节区的合并与重组、符号的重定位（确认符号在虚拟内存中的地址）等，把各个可重定向输入文件链接成一个可执行文件（或者是可共享文件）。如果可执行文件中使用了动态连接库，那么将包含一些用于动态符号链接的节区。

**注：节区头部表只对可重定向文件有用**

```c
typedef struct
{
Elf32_Word    sh_name;                /* Section name (string tbl index) */
Elf32_Word    sh_type;                /* Section type */
Elf32_Word    sh_flags;               /* Section flags */
Elf32_Addr    sh_addr;                /* Section virtual addr at execution */
Elf32_Off     sh_offset;              /* Section file offset */
Elf32_Word    sh_size;                /* Section size in bytes */
Elf32_Word    sh_link;                /* Link to another section */
Elf32_Word    sh_info;                /* Additional section information */
Elf32_Word    sh_addralign;           /* Section alignment */
Elf32_Word    sh_entsize;             /* Entry size if section holds table */
} Elf32_Shdr; 
```

* name：是节区头部字符串表节区（Section Header String Table Section）的索引。名字是一个 NULL 结尾的字符串。
* addr：如果节区将出现在进程的内存映像中，此成员给出节区的第一个字节应处的位置。否则，此字段为0
* offset：SHT_NOBITS 类型的节区不占用文件的空间，因此其 sh_offset 成员给出的是其概念性的偏移。
* size：类型为SHT_NOBITS 的节区长度可能非零，不过却不占用文件中的空间。
* link：记录SHT中的索引链接，意味着从这个字段出发，可以找到对应的另外两个section，具体解释依不同的section而不同
* addralign：地址对齐要求。例如，如果一个节区保存一个doubleword，那么系统必须保证整个节区能够按双字对齐。sh_addr对 sh_addralign 取模，结果必须为 0。目前仅允许取值为 0 和 2的幂次数。数值 0 和 1 表示节区没有对齐约束。
* entsize：某些节区中包含固定大小的项目，如符号表。对于这类节区，此成员给出每个表项的长度字节数。 如果节区中并不包含固定长度表项的表格，此成员取值为 0。

###**Program header table**

告诉系统如何创建进程映像，用来构造进程映像的目标文件必须具有程序头部表，只对可执行文件和共享文件有用，一个段可以包括多个节区

```c
typedef struct
{
Elf32_Word    p_type;                 /* Segment type */
Elf32_Off     p_offset;               /* Segment file offset */
Elf32_Addr    p_vaddr;                /* Segment virtual address */
Elf32_Addr    p_paddr;                /* Segment physical address */
Elf32_Word    p_filesz;               /* Segment size in file */
Elf32_Word    p_memsz;                /* Segment size in memory */
Elf32_Word    p_flags;                /* Segment flags */
Elf32_Word    p_align;                /* Segment alignment */
} Elf32_Phdr;
```

### **Section**

包含目标文件中除去ELF 头部、程序头部表格、节区头部表格的所有信息，满足以下条件：

1. 目标文件中的每个节区都有对应的节区头部描述它，反过来，有节区头部不意味着有节区。 
2. 每个节区占用文件中一个连续字节区域（这个区域可能长度为 0） 
3. 文件中的节区不能重叠，不允许一个字节存在于两个节区中的情况发生。 
4. 目标文件中可能包含非活动空间（INACTIVE SPACE）。这些区域不属于任何 头部和节区，其内容未指定。

#### Section Type

![](http://p5bpzbkxa.bkt.clouddn.com/%E8%8A%82%E5%8C%BA%E7%B1%BB%E5%9E%8B.png)

![](http://p5bpzbkxa.bkt.clouddn.com/%E8%8A%82%E5%8C%BA%E7%B1%BB%E5%9E%8B2.png)

#### Section flags 

![](http://p5bpzbkxa.bkt.clouddn.com/sh_flags.png)

#### Special Section

##### Program

* .text：程序的可执行指令
* .init：包含了可执行指令，是进程初始化代码的一部分。当程序开始执行时，系统要在 开始调用主程序入口之前（通常指C语言的main 函数）执行这些代码 

##### data

* .data/.datal：已初始化变量
* .bbs：未初始化变量，size为0
* .rotate/.rodatal：只读数据，常量字符串

注：函数内部的临时变量位于stack上，不在此列，这里的数据都是全局数据

##### 定义符号和引用符号的信息

* .symtab/.dynsym：符号表，对外宣布自己需要引用哪些符号，让linker帮忙解析，看是否有其他模块定义该符号
* .strtab/.dynstr：字符串信息
* .dynstr：区包含用于动态链接的字符串，大多数情况下这些字符串代表了与符号表项相关的名称。  

##### relocation information

* .rel：重定位条目，把若干个relocatable object file（当然还要有库文件）组织成一个可以被加载的image并分配正确的地址给各个符号 

##### 内嵌目标平台相关信息

##**String Table**

注：

* 字符串表本身就是一个节区，在elf文件头部结构中存在一个成员e_shstrndx给出这个节区头部表项的索引位置
* 字符串表节区包含以 NULL（ASCII 码 0）结尾的字符序列，通常称为字符串。ELF目标文件通常使用字符串来表示符号和节区名称。对字符串的引用通常以字符串在字符串表中的下标给出。
* 一般，第一个字节（索引为 0）定义为一个空字符串。类似的，字符串表的最后一个字节也定义为 NULL，以确保所有的字符串都以 NULL 结尾。索引为 0 的字符串在不同的上下文中可以表示无名或者名字为 NULL 的字符串。
* 允许存在空的字符串表节区，其节区头部的 sh_size 成员应该为 0。对空的字符串表而言，非 0 的索引值是非法的。

![](http://p5bpzbkxa.bkt.clouddn.com/string%20table.png)

### **Symbol Table**

目标文件的符号表中包含用来定位、重定位程序中符号定义和引用的信息。符号表 索引是对此数组的索引。索引 0 表示表中的第一表项，同时也作为未定义符号的索引。 符号表是由一个个符号元素组成，每个元素的数据结构如下定义：

```c
typedef struct
{
    Elf32_Word    st_name;                /* Symbol name (string tbl index) */
    Elf32_Addr    st_value;               /* Symbol value */
    Elf32_Word    st_size;                /* Symbol size */
    unsigned char st_info;                /* Symbol type and binding */
    unsigned char st_other;               /* Symbol visibility */
    Elf32_Section st_shndx;               /* Section index */
} Elf32_Sym;
```

#### st_info

包含符号类型和绑定信息，操纵方式如下所示

```c
#define ELF32_ST_BIND(i) ((i)>>4)
#define ELF32_ST_TYPE(i) ((i)&0xf)
#define ELF32_ST_INFO(b, t) (((b)<<4) + ((t)&0xf)) 
```

低四位表示符号绑定，用于确定链接可见性和行为，具体绑定类型如下所示：

|    名称    | 取值 |                             说明                             |
| :--------: | :--: | :----------------------------------------------------------: |
| STB_LOCAL  |  0   | 局部符号在包含该符号定义的目标文件以外不可见。相同名 称的局部符号可以存在于多个文件中，互不影响。 |
| STB_GLOBAL |  1   | 全局符号对所有将组合的目标文件都是可见的。一个文件中 对某个全局符号的定义将满足另一个文件对相同全局符号的 未定义引用。 |
|  STB_WEAK  |  2   |      弱符号与全局符号类似，不过他们的定义优先级比较低。      |
| STB_LOPROC |  13  |         处于这个范围的取值是保留给处理器专用语义的。         |
| STB_HIPROC |  15  |                             同上                             |

注：

* 当链接编辑器组合若干可重定位的目标文件时，不允许对同名的 STB_GLOBAL 符号给出多个定义。 另一方面如果一个已定义的全局符号已经存在，出现一个同名的弱符号并 不会产生错误。链接编辑器尽关心全局符号，忽略弱符号。 类似地，如果一个公共符号（符号的 st_shndx 中包含 SHN_COMMON），那 么具有相同名称的弱符号出现也不会导致错误。链接编辑器会采纳公共定 义，而忽略弱定义。 
* 当链接编辑器搜索归档库（archive libraries）时，会提取那些包含未定 义全局符号的档案成员。成员的定义可以是全局符号，也可以是弱符号。 连接编辑器不会提取档案成员来满足未定义的弱符号。 未能解析的弱符号取值为 0。  
* 在每个符号表中，所有具有 STB_LOCAL 绑定的符号都优先于弱符号和全局符 号。符号表节区中的 sh_info 头部成员包含第一个非局部符号的符号表索引。 

#### 符号类型



## ELF文件格式分析命令

- -a –all 全部 Equivalent to: -h -l -S -s -r -d -V -A -I
- -h –file-header 文件头 Display the ELF file header
- -l –program-headers 程序 Display the program headers
- –segments An alias for –program-headers
- -S –section-headers 段头 Display the sections’ header
- -e –headers 全部头 Equivalent to: -h -l -S
- -s –syms 符号表 Display the symbol table
- -n –notes 内核注释 Display the core notes (if present)
- -r –relocs 重定位 Display the relocations (if present)
- -u –unwind Display the unwind info (if present)
- -d –dynamic 动态段 Display the dynamic segment (if present)
- -V –version-info 版本 Display the version sections (if present)
- -A –arch-specific CPU构架 Display architecture specific information (if any).
- -D –use-dynamic 动态段 Use the dynamic section info when displaying symbols
- -x –hex-dump=<number> 显示 段内内容Dump the contents of section <number>
- -w[liaprmfFso] or
- -I –histogram Display histogram of bucket list lengths
- -W –wide 宽行输出 Allow output width to exceed 80 characters
- -H –help Display this information
- -v –version Display the version number of readelf

# 参考资料

* [readelf elf文件格式分析](http://linuxtools-rst.readthedocs.io/zh_CN/latest/tool/readelf.html)
* [Intel平台下Linux中ELF文件动态链接的加载、解析及实例分析（一） ](https://www.ibm.com/developerworks/cn/linux/l-elf/part1/index.html)
* [C语言编程透视](https://www.kancloud.cn/kancloud/cbook/68993)
* [可执行文件（ELF）格式的理解](https://www.cnblogs.com/xmphoenix/archive/2011/10/23/2221879.html)
* [elf.h Source File](http://www.openvirtualization.org/documentation/elf_8h_source.html)
* [计算机科学基础知识（二）:Relocatable Object File](http://www.wowotech.net/basic_subject/compile-link-load.html)
* [ELF文件格式分析](https://download.csdn.net/download/jiangwei0910410003/9204051)
* [ELF文件格式总结](https://blog.csdn.net/flydream0/article/details/8719036)

