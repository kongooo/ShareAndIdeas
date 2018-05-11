[TOC]

# Linux ELF文件

## 作用

用于定义不同类型的对象文件(Object files)中都放了什么东西、以及都以什么样的格式去放这些东西 

## 类型

* **可重定位**的对象文件：由汇编器汇编生成的.o文件
* **可执行**的对象文件：可执行应用程序
* **可被共享**的对象文件：动态库文件，即.so文件

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

告诉系统如何创建进程映像，用来构造进程映像的目标文件必须具有程序头部表，可执行文件或者共享目标文件的程序头部是一个结构数组，每个结构描述了一个段 或者系统准备程序执行所必需的其它信息。目标文件的“段”包含一个或者多个“节区”， 也就是“段内容（Segment Contents）”。程序头部仅对于可执行文件和共享目标文件有意义。 

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

p_align：可加载的进程段的 p_vaddr 和 p_offset 取值必须合适，相对于对页面大小的取模而言。此成员给出段在文件中和内存中如何 对齐。数值0和1表示不需要对齐。否则 p_align 应该是个正整数，并且是2的幂次数，p_vaddr 和 p_offset 对 p_align取模后应该相等。 

#### 段类型

| 名字       |    取值    | 说明                                                         |
| ---------- | :--------: | :----------------------------------------------------------- |
| PT_NULL    |     0      | 此数组元素未用。结构中其他成员都是未定义的。                 |
| PT_LOAD    |     1      | 此数组元素给出一个可加载的段，段的大小由 p_filesz 和 p_memsz 描述。文件中的字节被映射到内存段开始处。如果 p_memsz 大于 p_filesz，“剩余”的字节要清零。p_filesz 不能大于 p_memsz。可加载 的段在程序头部表格中根据 p_vaddr 成员按升序排列。 |
| PT_DYNAMIC |     2      | 数组元素给出动态链接信息。                                   |
| PT_INTERP  |     3      | 数组元素给出一个 NULL 结尾的字符串的位置和长度，该字符串将被 当作解释器调用。这种段类型仅对与可执行文件有意义（尽管也可能 在共享目标文件上发生）。在一个文件中不能出现一次以上。如果存在 这种类型的段，它必须在所有可加载段项目的前面。 |
| PT_NOTE    |     4      | 此数组元素给出附加信息的位置和大小。                         |
| PT_SHLIB   |     5      | 此段类型被保留，不过语义未指定。包含这种类型的段的程序与 ABI 不符。 |
| PT_PHDR    |     6      | 此类型的数组元素如果存在，则给出了程序头部表自身的大小和位置， 既包括在文件中也包括在内存中的信息。此类型的段在文件中不能出 现一次以上。并且只有程序头部表是程序的内存映像的一部分时才起 作用。如果存在此类型段，则必须在所有可加载段项目的前面。 |
| PT_LOPROC  | 0x70000000 | 此范围的类型保留给处理器专用语义。                           |
| PT_HIPROC  | 0x7fffffff | 同上                                                         |

#### 基地址

基地址用来对程序的内存映像进行重定位。可执行文件或者共享目标文件的基地址 是在执行过程中从三个数值计算的： 

* 内存加载地址 
* 最大页面大小 
* 程序的可加载段的最低虚地址。 

程序头部中的虚拟地址可能不能代表程序内存映像的实际虚地址。要计算基地址， 首先要确定与 PT_LOAD 段的最低 p_vaddr 值相关的内存地址。通过对内存地址向最接 近的最大页面大小截断，就可以得到基地址。根据要加载到内存中的文件的类型，内存 地址可能与 p_vaddr 相同也可能不同。

如前所述，“.bss”节区的类型为 SHT_NOBITS。尽管它在文件中不占据空间，却会 占据段的内存映像的空间。通常，这些未初始化的数据位于段的末尾，所以 p_memsz 会比 p_filesz 大。  

### **Section**

包含目标文件中除去ELF 头部、程序头部表格、节区头部表格的所有信息，比如程序的正文区（代码）、数据区（初始化和未初始化的数据）、调试信息、以及用于动态链接的一些节区，比如解释器（`.interp`）节区将指定程序动态装载 `/` 链接器 `ld-linux.so` 的位置，而过程链接表（`plt`）、全局偏移表（`got`）、重定位表则用于辅助动态链接过程。满足以下条件：

1. 目标文件中的每个节区都有对应的节区头部描述它，反过来，有节区头部不意味着有节区。 
2. 每个节区占用文件中一个连续字节区域（这个区域可能长度为 0） 
3. 文件中的节区不能重叠，不允许一个字节存在于两个节区中的情况发生。 
4. 目标文件中可能包含非活动空间（INACTIVE SPACE）。这些区域不属于任何 头部和节区，其内容未指定。

#### Section Type

![](http://p5bpzbkxa.bkt.clouddn.com/%E8%8A%82%E5%8C%BA%E7%B1%BB%E5%9E%8B.png)

![](http://p5bpzbkxa.bkt.clouddn.com/%E8%8A%82%E5%8C%BA%E7%B1%BB%E5%9E%8B2.png)

#### Section flags 

```c
/* Legal values for sh_flags (section flags).  */

#define SHF_WRITE            (1 << 0)   /* Writable */
#define SHF_ALLOC            (1 << 1)   /* Occupies memory during execution */
#define SHF_EXECINSTR        (1 << 2)   /* Executable */
#define SHF_MERGE            (1 << 4)   /* Might be merged */
#define SHF_STRINGS          (1 << 5)   /* Contains nul-terminated strings */
#define SHF_INFO_LINK        (1 << 6)   /* `sh_info' contains SHT index */
#define SHF_LINK_ORDER       (1 << 7)   /* Preserve order after combining */
#define SHF_OS_NONCONFORMING (1 << 8)   /* Non-standard OS specific handling required */
#define SHF_GROUP            (1 << 9)   /* Section is member of a group.  */
#define SHF_TLS              (1 << 10)  /* Section hold thread-local data.  */
#define SHF_MASKOS           0x0ff00000 /* OS-specific.  */
#define SHF_MASKPROC         0xf0000000 /* Processor-specific */
#define SHF_ORDERED          (1 << 30)  /* Special ordering requirement(Solaris).  */
#define SHF_EXCLUDE          (1 << 31)  /* Section is excluded unless referenced or allocated (Solaris).*/
```

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
* .dynamic：存放了和动态链接的很多信息，例如动态链接器通过它找到该文件使用的动态链接库 
* .interp：专门制定动态连接器的节区

##### 内嵌目标平台相关信息

### **String Table**

注：

* 字符串表本身就是一个节区，在elf文件头部结构中存在一个成员e_shstrndx给出这个节区头部表项的索引位置
* 字符串表节区包含以 NULL（ASCII 码 0）结尾的字符序列，通常称为字符串。ELF目标文件通常使用字符串来表示符号和节区名称。对字符串的引用通常以字符串在字符串表中的下标给出。
* 一般，第一个字节（索引为 0）定义为一个空字符串。类似的，字符串表的最后一个字节也定义为 NULL，以确保所有的字符串都以 NULL 结尾。索引为 0 的字符串在不同的上下文中可以表示无名或者名字为 NULL 的字符串。
* 允许存在空的字符串表节区，其节区头部的 sh_size 成员应该为 0。对空的字符串表而言，非 0 的索引值是非法的。

![](http://p5bpzbkxa.bkt.clouddn.com/string%20table.png)

### **Symbol Table**

对于可执行文件除了编译器引入的一些符号外，主要就是用户自定义的全局变量，函数等，而对于可重定位文件仅仅包含用户自定义的一些符号。 

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

|    名称    | 取值 | 说明                                                         |
| :--------: | :--: | :----------------------------------------------------------- |
| STB_LOCAL  |  0   | 局部符号在包含该符号定义的目标文件以外不可见。相同名称的局部符号可以存在于多个文件中，互不影响。 |
| STB_GLOBAL |  1   | 全局符号对所有将组合的目标文件都是可见的。一个文件中对某个全局符号的定义将满足另一个文件对相同全局符号的 未定义引用。 |
|  STB_WEAK  |  2   | 弱符号与全局符号类似，不过他们的定义优先级比较低。           |
| STB_LOPROC |  13  | 处于这个范围的取值是保留给处理器专用语义的。                 |
| STB_HIPROC |  15  | 同上                                                         |

注：

* 当链接编辑器组合若干可重定位的目标文件时，不允许对同名的 STB_GLOBAL 符号给出多个定义。 另一方面如果一个已定义的全局符号已经存在，出现一个同名的弱符号并不会产生错误。链接编辑器尽关心全局符号，忽略弱符号。 类似地，如果一个公共符号（符号的 st_shndx 中包含SHN_COMMON），那 么具有相同名称的弱符号出现也不会导致错误。链接编辑器会采纳公共定 义，而忽略弱定义。 
* 当链接编辑器搜索归档库（archive libraries）时，会提取那些包含未定 义全局符号的档案成员。成员的定义可以是全局符号，也可以是弱符号。 连接编辑器不会提取档案成员来满足未定义的弱符号。 未能解析的弱符号取值为 0。  
* 在每个符号表中，所有具有STB_LOCAL 绑定的符号都优先于弱符号和全局符 号。符号表节区中的sh_info头部成员包含第一个非局部符号的符号表索引。 

#### 符号类型

|    名称     | 取值 | 说明                                                         |
| :---------: | :--: | :----------------------------------------------------------- |
| STT_NOTYPE  |  0   | 符号的类型没有指定                                           |
| STT_OBJECT  |  1   | 符号与某个数据对象相关，比如一个变量、数组等等               |
|  STT_FUNC   |  2   | 符号与某个函数或者其他可执行代码相关                         |
| STT_SECTION |  3   | 符号与某个节区相关。这种类型的符号表项主要用于重定 位，通常具有 STB_LOCAL 绑定。 |
|  STT_FILE   |  4   | 传统上，符号的名称给出了与目标文件相关的源文件的名 称。文件符号具有 STB_LOCAL 绑定，其节区索引是  SHN_ABS，并且它优先于文件的其他 STB_LOCAL 符号 （如果有的话） |
|  STT_LOPRO  |  13  | 此范围的符号类型值保留给处理器专用语义用途                   |
| STT_HIPROC  |  15  | 同上                                                         |

注：

* 在共享目标文件中的函数符号（类型为 STT_FUNC）具有特别的重要性。当其他 目标文件引用了来自某个共享目标中的函数时，链接编辑器自动为所引用的符号创建过 程链接表项。类型不是 STT_FUNC 的共享目标符号不会自动通过过程链接表进行引 用。 
* 如果一个符号的取值引用了某个节区中的特定位置，那么它的节区索引成员 （st_shndx）包含了其在节区头部表中的索引。当节区在重定位过程中被移动时，符号 的取值也会随之变化，对符号的引用始终会“指向”程序中的相同位置。 
* LOCAL符号不是临时变量，临时变量是放在stack中的，不会出现在符号表中 
* 所有定义在函数内部的变量都是临时变量，如果前面有static的修饰符，那么该变量虽然作用域是函数内部，但是也会出现在符号表中，只不过名字会是源程序中的符号附加一个“.xxxx”，xxxx是一个数字 

#### 特殊的节区索引

* SHN_ABS： 符号具有绝对取值，不会因为重定位而发生变化
* SHN_COMMON： 符号标注了一个尚未分配的公共块。符号的取值给出了对齐约束，与节区的sh_addralign 成员类似。就是说，链接编辑器将为符号分配存储空间， 地址位于st_value 的倍数处。符号的大小给出了所需要的字节数。 
* SHN_UNDEF： 此节区表索引值意味着符号没有定义。 当链接编辑器将此目标文件与其他定义了该符号的目标 文件进行组合时，此文件中对该符号的引用将被链接到实 际定义的位置。 

#### STN_UNDEF符号

|   名称   | 取值 |       说明       |
| :------: | :--: | :--------------: |
| st_name  |  0   |      无名称      |
| st_value |  0   |       0值        |
| st_size  |  0   |      无大小      |
| st_info  |  0   | 无类型，局部绑定 |
| st_other |  0   |    无附加信息    |
| st_shndx |  0   |      无节区      |

#### 符号取值

不同的目标类型文件中符号表项对st_value成员有不同的解释：

* 在可重定位文件中，st_value 中遵从了节区索引为 SHN_COMMON 的符号的对齐约束。 
* 在可重定位的文件中，st_value 中包含已定义符号的节区偏移。就是说， st_value 是从 st_shndx 所标识的节区头部开始计算，到符号位置的偏 移。  
* 在可执行和共享目标文件中，st_value 包含一个虚地址。为了使得这些 文件的符号对动态链接器更有用，节区偏移（针对文 件的解释）让位于 虚拟地址（针对内存的解释），因为这时与节区号无关。  

### **重定位信息**

重定位是将符号引用与符号定义进行链接的过程。例如，当程序调用了一个函数时， 相关的调用指令必须把控制传输到适当的目标执行地址。 

#### 重定位表项

可重定位文件必须包含如何修改其节区内容的信息，从而允许可执行文件和共享目 标文件保存进程的程序映像的正确信息。重定位表项就是这样一些数据。 格式如下图所示：

```c
/* I have seen two different definitions of the Elf64_Rel and Elf64_Rela structures, so we'll leave them out until Novell (or whoever) gets their act together.  */
/* The following, at least, is used on Sparc v9, MIPS, and Alpha.  */

typedef struct
{
Elf64_Addr    r_offset;               /* Address */
Elf64_Xword   r_info;                 /* Relocation type and symbol index */
} Elf64_Rel;

/* Relocation table entry with addend (in section of type SHT_RELA).  */

typedef struct
{
Elf32_Addr    r_offset;               /* Address */
Elf32_Word    r_info;                 /* Relocation type and symbol index */
Elf32_Sword   r_addend;               /* Addend */
} Elf32_Rela;

/* How to extract and insert information held in the r_info field.  */

#define ELF32_R_SYM(val)                ((val) >> 8)
#define ELF32_R_TYPE(val)               ((val) & 0xff)
#define ELF32_R_INFO(sym, type)         (((sym) << 8) + ((type) & 0xff))

#define ELF64_R_SYM(i)                  ((i) >> 32)
#define ELF64_R_TYPE(i)                 ((i) & 0xffffffff)
#define ELF64_R_INFO(sym,type)          ((((Elf64_Xword) (sym)) << 32) + (type))
```

字段说明：

* r_offset：此成员给出了重定位动作所适用的位置。对于一个可重定位文件而言， 此值是从节区头部开始到将被重定位影响的存储单位之间的字节偏 移。对于可执行文件或者共享目标文件而言，其取值是被重定位影响 到的存储单元的虚拟地址。  
* r_info：此成员给出要进行重定位的符号表索引重定位表项引用到的符号表） ，以及将实施的重定位类型（如何进行符号的重定位） 。 例如一个调用指令的重定位项将包含被调用函数的符号表索引。如果 索引是 STN_UNDEF，那么重定位使用 0 作为“符号值”。重定位类型是和处理器相关的。当程序代码引用一个重定位项的重定位类型或 者符号表索引，则表示对表项的 r_info 成员应用 ELF32_R_TYPE 或 者 ELF32_R_SYM 的结果。 
* r_addend：此成员给出一个常量补齐，用来计算将被填充到可重定位字段的数值。 （Elf32_Rela 项目可以明确包含补齐信息。类型为 Elf32_Rel 的表项在将被修改的位置保存隐式的补齐信息 ）

重定位节区会引用两个其它节区：符号表、要修改的节区。节区头部的 sh_info 和 sh_link 成员给出这些关系。不同目标文件的重定位表项对 r_offset 成员具有略微不同的解释。

* 在可重定位文件中，r_offset 中包含节区偏移。就是说重定位节区自身 描述了如何修改文件中的其他节区；重定位偏移 指定了被修改节区中的 一个存储单元。 
* 在可执行文件和共享的目标文件中，r_offset 中包含一个虚拟地址。为 了使得这些文件的重定位表项对动态链接器更为有用，节区偏移（针对文 件的解释）让位于虚地址（针对内存的解释）。  

#### 重定位类型

给出哪些位需要修改以及如何计算他们的取值

##### 标记说明

* **A** 用来计算可重定位字段的取值的补齐。
*  **B** 共享目标在执行过程中被加载到内存中的位置（基地址）。
*  **G** 在执行过程中，重定位项的符号的地址所处的位置 —— 全局偏移 表的索引。
*  **GOT** 全局偏移表（GOT）的地址。 
* **L** 某个符号的过程链接表项的位置（节区偏移/地址）。过程链接表项 把函数调用重定位到正确的目标位置。链接编辑器构造初始的过程链 接表，动态链接器在执行过程中修改这些项目。 
* **P** 存储单位被重定位（用 r_offset 计算）到的位置（节区偏移或者地 址）。
*  **S** 其索引位于重定位项中的符号的取值。 

##### X86体系结构下常见重定位类型

| 名称           | 数值 | 字段   | 计算    | 说明                                                         |
| -------------- | :--: | :----- | :------ | ------------------------------------------------------------ |
| R_386_NONE     |  0   |        |         |                                                              |
| R_386_32       |  1   | word32 | S+A     |                                                              |
| R_386_PC32     |  2   | word32 | S+A_P   |                                                              |
| R_386_GOT32    |  3   | word32 | G+A_P   | 此重定位类型计算从全局偏移表基址到符号 的全局偏移表项之间的距离。它会通知连接编 辑器构造一个全局偏移表。 |
| R_386_PLT32    |  4   | word32 | L+A_P   | 此重定位类型计算符号的过程链接表项的地 质，并通知链接编辑器构造一个过程链接表。 |
| R_386_COPY     |  5   |        |         | 链接编辑器创建这种重定位类型的目的是支 持动态链接。其偏移量成员引用某个可写段中 的某个位置。符号表索引规定符号应该既存在 于当前目标文件中，也存在于某个共享目标 中。在执行过程中，动态链接器把与共享目标 的符号相关的数据复制到由偏移给出的位置。 |
| R_386_GLOB_DAT |  6   | word32 | S       | 此重定位类型用来把某个全局偏移表项设置 为给定符号的地址。这种特殊的重定位类型允 许确定符号与全局偏移表项之间的关系。 |
| R_386_JMP_SLOT |  7   | word32 | S       | 链接编辑器创建这种重定位类型主要是为了 支持动态链接。其偏移地址成员给出过程链接 表项的位置。动态链接器修改过程链接表项的 内容，把控制传输给指定符号的地址。 |
| R_386_RELATIVE |  8   | word32 | B+A     | 链接编辑器创建这种重定位类型是为了支持 动态链接。其偏移地址成员给出共享目标中的 一个位置，在该位置包含了代表相对地址的一 个数值。动态链接器通过把共享目标被加载到 的虚地址和相对地址相加，计算对应的虚地 址。这种类型的重定位项必须设置符号表索引 为 0 |
| R_386_GOTOFF   |  9   | word32 | S+A_GOT | 这种重定位类型会计算符号取值与全局偏移 表地址间的差。并通知链接编辑器创建一个全 局偏移表。 |
| R_386_GOTPC    |  10  | word32 | GOT+A_P | 此重定位类型与 R_386_PC32 类似，只不过 它在计算时采用全局偏移表的地址。在此重定 位项中 引用的 符号通 常 是 _GLOBAL_OFFSET_TABLE_，这种类型也会 暗示连接编辑器构造全局偏移表。 |

#### 程序加载和动态链接

主要技术：

* 程序头部
* 程序加载：给定一个目标文件，系统加载该文件到内存中，启动程序执行。 
* 动态链接：系统加载了程序以后，必须通过解析构成进程的目标文件之间的符 号引用，以便完整地构造进程映像。 

##### 程序加载

进程除非在执行过程中引用到相应的逻辑页面，否则不会请求真正的物理页面。进 程通常会包含很多未引用的页面，因此，延迟物理读操作通常会避免这类费力不讨好的 事情发生，从而提高系统性能。要想实际获得这种效率，可执行文件和共享目标文件必 须具有这样的段：其文件偏移和虚拟地址对页面大小取模后余数相同。 

可执行文件与共享文件在段加载上的差异：

* 可执行文件的段通常包 含绝对代码，为了能够让进程正确执行，所使用的段必须是构造可执行文件时所使用的 虚拟地址。因此系统会使用 p_vaddr 作为虚拟地址。 
* 共享目标文件的段通常包含与位置无关的代码。这使得段的虚拟地址在不同的进程中不同， 但不影响执行行为。尽管系统为每个进程选择独立的虚拟地址，仍能维持段的相对位置。因为位置 独立的代码在段与段之间使用相对寻址，内存虚地址之间的差异必须与文件中虚拟地址之间的差异 相匹配。 

##### 动态链接

动态链接就是在程序运行时对符号进行重定位，确定符号对应的内存地址的过程。 

Linux 下符号的动态链接默认采用[Lazy Mode方式](http://elfhack.whitecell.org/mydocs/ELF_symbol_resolve_process1.txt)，也就是说在程序运行过程中用到该符号时才去解析它的地址。这样一种符号解析方式有一个好处：只解析那些用到的符号，而对那些不用的符号则永远不用解析，从而提高程序的执行效率。

不过这种默认是可以通过设置 `LD_BIND_NOW` 为非空来打破的，也就是说如果设置了这个变量，动态链接器将在程序加载后和符号被使用之前就对这些符号的地址进行解析

###### 动态链接库

上面提到重定位的过程就是对符号引用和符号地址进行链接的过程，而动态链接过程涉及到的符号引用和符号定义分别对应可执行文件和动态链接库，在可执行文件中可能引用了某些动态链接库中定义的符号，这类符号通常是函数。

为了让动态链接器能够进行符号的重定位，必须把动态链接库的相关信息写入到可执行文件当中。

###### 动态链接器

inux 下 `elf` 文件的动态链接器是 `ld-linux.so`，即 `/lib/ld-linux.so.2` 

程序被执行时，`ld-linux.so` 将最先被装载到内存中，没有其他程序知道去哪里查找 `ld-linux.so`，所以它的路径必须是绝对的；当 `ld-linux.so` 被装载以后，由它来去装载可执行文件和相关的共享库，它将根据 `PATH` 变量和 `LD_LIBRARY_PATH` 变量去磁盘上查找它们，因此可执行文件和共享库都可以不指定绝对路径。 

###### 程序解释器

什么是解释器？

解释器是一个函数，你输入一个“表达式”，它就输出一个“值”。表达式是一种“表象”或“符号”，值更接近“本质”或“意义”。我们“解释了”符号，得到它的意义。需要注意的是，表达式是一个数据结构，而不是一个字符串。我们用一种叫“S 表达式”（S-expression）的结构来存储表达式。比如表达式 `'(+ 1 2)` 其实是一个链表（list），它里面的内容是三个符号（symbol）：`+`, `1` 和 `2`，而不是字符串`"(+ 1 2)"` 

在 exec() 期间，系统从 PT_INTERP 段中检索路径名，并从解释器文件的段创建初始的进程映像。也就是说， 系统并不使用原来可执行文件的段映像，而是为解释器构造一个内存映像。接下来是解释器从系统接收控制，为应用程序提供执行环境 

解释器接受控制的两种方式：

* 接受一个文件描述符，读取可执行文件并将其映射到内存中 
* 根据可执行文件的格式，系统可能把可执行文件加载到内存中，而不是为解释器提 供一个已经打开的文件描述符。 

解释器可以是一个可执行文件，也可以是一个共享目标文件。 

共享目标文件被加载到内存中时，其地址可能在各个进程中呈现不同的取值。系统 在 mmap 以及相关服务所使用的动态段区域创建共享目标文件的段。因此，共享目标 解释器通常不会与原来的可执行文件的原始段地址发生冲突。 

可执行文件被加载到内存中固定地址，系统使用来自其程序头部表的虚拟地址创建 各个段。因此，可执行文件解释器的虚拟地址可能会与原来的可执行文件的虚拟地址发 生冲突。解释器要负责解决这种冲突。 

###### 动态加载程序

在构造使用动态链接技术的可执行文件时，连接编辑器向可执行文件中添加一个类 型为 PT_INTERP 的程序头部元素，告诉系统要把动态链接器激活，作为程序解释器。系 统所提供的动态链接器的位置是和处理器相关的。 

Exec() 和动态链接器合作，为程序创建进程映像（exec()之后程序指令运行之前），其中包括以下动作：

1. 将可执行文件的内存段添加到进程映像中
2. 把共享目标内存段添加到进程映像中
3. 为可执行文件和它的共享目标（动态链接库）执行重定位操作
4. 关闭用来读入可执行文件的文件描述符，如果动态链接程序收到过这样的 文件描述符的话 
5. 将控制转交给程序，使得程序好像从 exec 直接得到控制  

下面对各个步骤进行详细解释：

1. 在 `ELF` 文件的文件头中就指定了该文件的入口地址，程序的代码和数据部分会相继 `map` 到对应的内存中。而关于可执行文件本身的路径，如果指定了 `PATH` 环境变量，`ld-linux` 会到 `PATH` 指定的相关目录下查找。 
2. `.dynamic` 节区指定了可执行文件依赖的库名，`ld-linux` （程序解释器/动态装载器）再从 `LD_LIBRARY_PATH` 指定的路径中找到相关的库文件或者直接从 `/etc/ld.so.cache` 库缓冲中加载相关库到内存中。 
3. 如果设置了 `LD_BIND_NOW` 环境变量，这个动作就会在此时发生，否则将会采用 `lazy mode` 方式，即当某个符号被使用时才会进行符号的重定位。不过无论在什么时候发生这个动作，重定位的过程大体是一样的 
4. 释放文件描述符 
5. 动态链接器把程序控制权交还给程序 

###### 动态节区

如果一个目标文件参与动态链接，它的程序头部表将包含类型为 PT_DYNAMIC 的元 素。此“段”包含.dynamic 节区。该节区采用一个特殊符号_DYNAMIC 来标记，其中包 含如下结构的数组。  

```c
typedef struct
{
    Elf32_Sword   d_tag;                  /* Dynamic entry type */
    union
    {
        Elf32_Word d_val;                 /* Integer value */
        Elf32_Addr d_ptr;                 /* Address value */
    } d_un;
} Elf32_Dyn;
```

* d_val：表示一个整数值，可以有多种解释。 

* d_ptr：代表程序的虚拟地址。如前所述，文件的 虚拟地址可能与执行过程中的内存虚地址不匹配。在解释包含于动态 结构中的地址时，动态链接程序基于原来文件值和内存基地址计算实 际地址。为了保持一致性，文件中不包含用来“纠正”动态结构中重 定位项地址的重定位项目。

   

###### 全局偏移表（GOT）

在私有数据中包含绝对地址，从而使得地址可用，并且不会影响位置独立性和程序代码的可共享性。程序使用位置独立的寻址引用其全局偏移表,并取得绝对值,从而把位置独立的引用重定向到绝对位置。

全局偏移表中最初包含其重定位项中要求的信息。如果程序需要直接访问某个符号的绝对地址,那么该符号就会具有一个全局偏移表项。由于可执行文件和共享目标具有独立的全局偏移表,一个符号的地址可能出现在多个表中。动态链接器在将控制交给进程映像中任何代码之前,要处理所有的全局偏移表重定位,因而确保了执行过程中绝对地址信息可用。

###### 过程链接表（PLT）

把位置独立的函数调用重定向到绝对位置

动态链接器能够确定目标处的绝对地址,并据此修改全局偏移表的内存映像。动态
链接器因此能够对表项进行重定位,并且不会影响程序代码的位置独立性和可共享性。可执行文件和共享目标文件拥有各自独立的过程链接表。

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

