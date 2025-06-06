#pragma once

#if defined(__i386__)
#define ARCHITECTURE_IS_X86 1
#elif defined(__x86_64__)
#define ARCHITECTURE_IS_AMD64 1
#elif defined(__arm__)
#define ARCHITECTURE_IS_ARMv7 1
#define ARM_FPU_VFP 1
#define HAVE_ARMV6 1
#elif defined(__aarch64__)
#define ARCHITECTURE_IS_AARCH64 1
#else
#error Unknown architecture when building!
#endif

//Configurable defines for this platform
#define G_DIR_SEPARATOR_IS_FORWARDSLASH 1
#define G_HAVE_ISO_VARARGS 1
#define GLIBC_BEFORE_2_3_4_SCHED_SETAFFINITY 1
#define GLIBC_HAS_CPU_COUNT 1
#define GPID_IS_INT 1
#define G_SEARCHPATH_SEPARATOR_IS_COLON 1
#define MAJOR_IN_SYSMACROS 1
#define HAVE_ARRAY_ELEM_INIT 1
#define HAVE_BLKCNT_T 1
#define HAVE_BLKSIZE_T 1
#define HAVE_CLASSIC_WINAPI_SUPPORT 1
#define HAVE_CLOCK_NANOSLEEP 1
#define HAVE_CRYPT_RNG 1
#define HAVE_DEV_RANDOM 1
#define HAVE_DLADDR 1
#define HAVE_DL_LOADER 1
#define HAVE_EPOLL 1
#define HAVE_EPOLL_CTL 1
#define HAVE_EXECV 1
#define HAVE_EXECVE 1
#define HAVE_EXECVP 1
#define HAVE_FINITE 1
#define HAVE_FORK 1
#define HAVE_FSTATAT 1
#define HAVE_FSTATFS 1
#define HAVE_GETADDRINFO 1
#define HAVE_GETHOSTBYNAME 1
#define HAVE_GETHOSTBYNAME2 1
#define HAVE_GETNAMEINFO 1
#define HAVE_GETPRIORITY 1
#define HAVE_GETPROTOBYNAME 1
#define HAVE_GETRESUID 1
#define HAVE_GETRLIMIT 1
#define HAVE_GETRUSAGE 1
#define HAVE_GNUC_NORETURN 1
#define HAVE_GNUC_UNUSED 1
#define HAVE_IF_NAMETOINDEX 1
#define HAVE_INET_ATON 1
#define HAVE_INET_NTOP 1
#define HAVE_INET_PTON 1
#define HAVE_IPPROTO_IP 1
#define HAVE_IPPROTO_IPV6 1
#define HAVE_IPPROTO_TCP 1
#define HAVE_IP_MTU_DISCOVER 1
#define HAVE_IP_PKTINFO 1
#define HAVE_IP_PMTUDISC_DO 1
#define HAVE_ISFINITE 1
#define HAVE_ISINF 1
#define HAVE_KILL 1
#define HAVE_LARGE_FILE_SUPPORT 1
// We're depending on the POSIX implementation instead:
// #define HAVE_MADVISE 0
// This is referenced in e.g. external\mono\mono\utils\mono-mmap.c
// but not available on QNX:
// #define HAVE_MINCORE 0
#define HAVE_MKSTEMP 1
#define HAVE_MMAP 1
#define HAVE_MOVING_COLLECTOR 1
#define HAVE_MREMAP 1
#define HAVE_MSG_NOSIGNAL 1
#define HAVE_POLL 1
#define HAVE_PRCTL 1
#define HAVE_PTHREAD_ATTR_GETSTACK 1
#define HAVE_PTHREAD_ATTR_GETSTACKSIZE 1
#define HAVE_PTHREAD_ATTR_SETSTACKSIZE 1
#define HAVE_PTHREAD_GETATTR_NP 1
#define HAVE_PTHREAD_KILL 1
#define HAVE_PTHREAD_MUTEX_TIMEDLOCK 1
#define HAVE_PTHREAD_SETNAME_NP 1
#define HAVE_READV 1
#define HAVE_REWINDDIR 1
#define HAVE_SETGROUPS 1
#define HAVE_SETPGID 1
#define HAVE_SETPRIORITY 1
#define HAVE_SETRESUID 1
#define HAVE_SIGNAL 1
#define HAVE_SIOCGIFCONF 1
#define HAVE_SOCKLEN_T 1
#define HAVE_SOL_IP 1
#define HAVE_SOL_IPV6 1
#define HAVE_SOL_TCP 1
#define HAVE_STATFS 1
#define HAVE_STRNDUP 1
#define HAVE_STRTOK_R 1
#define HAVE_STRERROR_R 1
#define HAVE_STRUCT_CMSGHDR 1
#define HAVE_STRUCT_DIRENT_D_OFF 1
#define HAVE_STRUCT_DIRENT_D_RECLEN 1
#define HAVE_STRUCT_DIRENT_D_TYPE 1
#define HAVE_STRUCT_FLOCK 1
#define HAVE_STRUCT_IOVEC 1
#define HAVE_STRUCT_IP_MREQN 1
#define HAVE_STRUCT_LINGER 1
#define HAVE_STRUCT_POLLFD 1
#define HAVE_STRUCT_SOCKADDR 1
#define HAVE_STRUCT_SOCKADDR_IN 1
#define HAVE_STRUCT_SOCKADDR_IN6 1
#define HAVE_STRUCT_SOCKADDR_STORAGE 1
#define HAVE_STRUCT_SOCKADDR_UN 1
#define HAVE_STRUCT_STAT 1
#define HAVE_STRUCT_STATFS_F_FLAGS 1
#define HAVE_STRUCT_TIMESPEC 1
#define HAVE_STRUCT_TIMEVAL 1
#define HAVE_STRUCT_TIMEZONE 1
#define HAVE_STRUCT_UTIMBUF 1
#define HAVE_SUSECONDS_T 1
#define HAVE_SYSCONF 1
#define HAVE_SYSTEM 1
#define HAVE_SYS_ZLIB 1
#define HAVE_TM_GMTOFF 1
#define HAVE_TRUNC 1
#define HAVE_TTYNAME_R 1
#define HAVE_UWP_WINAPI_SUPPORT 0
#define HAVE_VISIBILITY_HIDDEN 1
#define HAVE_VSNPRINTF 1
#define HAVE_WRITEV 1
#define HAVE_ZLIB 1
#define LT_OBJDIR ".libs/"
#define MONO_XEN_OPT 1
#define MONO_ZERO_LEN_ARRAY 0
#ifndef PAGE_SIZE
#define PAGE_SIZE 0x1000
#endif
#define _POSIX_PATH_MAX 256
#define PLATFORM_IS_LITTLE_ENDIAN 1
#define S_IWRITE S_IWUSR
#define USE_GCC_ATOMIC_OPS 1

//Available headers for this platform
#define HAVE_DLFCN_H 1
#define HAVE_GETOPT_H 1
#define HAVE_INTTYPES_H 1
#define HAVE_MEMORY_H 1
#define HAVE_PWD_H 1
#define HAVE_STDINT_H 1
#define HAVE_STDLIB_H 1
#define HAVE_STRINGS_H 1
#define HAVE_STRING_H 1
#define HAVE_SYS_RESOURCE_H 1
#define HAVE_SYS_SELECT_H 1
#define HAVE_SYS_STAT_H 1
#define HAVE_SYS_TIME_H 1
#define HAVE_SYS_TYPES_H 1
#define HAVE_SYS_WAIT_H 1
#define HAVE_UNISTD_H 1
#define HAVE_ALLOCA_H 1
#define HAVE_ARPA_INET_H 1
#define HAVE_ASM_SIGCONTEXT_H 1
#define HAVE_DIRENT_H 1
#define HAVE_DLFCN_H 1
#define HAVE_ELF_H 1
#define HAVE_GRP_H 1
#define HAVE_INTTYPES_H 1
#define HAVE_LINK_H 1
#define HAVE_LINUX_MAGIC_H 1
#define HAVE_LINUX_NETLINK_H 1
#define HAVE_LINUX_RTC_H 1
#define HAVE_LINUX_RTNETLINK_H 1
#define HAVE_MEMORY_H 1
#define HAVE_NETDB_H 1
#define HAVE_NETINET_IN_H 1
#define HAVE_NETINET_TCP_H 1
#define HAVE_NET_IF_H 1
#define HAVE_PATHCONF_H 1
#define HAVE_POLL_H 1
#define HAVE_PTHREAD_H 1
#define HAVE_PWD_H 1
#define HAVE_SEMAPHORE_H 1
#define HAVE_SIGNAL_H 1
#define HAVE_STDINT_H 1
#define HAVE_STDLIB_H 1
#define HAVE_STRINGS_H 1
#define HAVE_STRING_H 1
#define HAVE_SYSLOG_H 1
#define HAVE_SYS_EPOLL_H 1
#define HAVE_SYS_INOTIFY_H 1
#define HAVE_SYS_IOCTL_H 1
#define HAVE_SYS_IPC_H 1
#define HAVE_SYS_MMAN_H 1
#define HAVE_SYS_MOUNT_H 1
#define HAVE_SYS_PARAM_H 1
#define HAVE_SYS_POLL_H 1
#define HAVE_SYS_PRCTL_H 1
#define HAVE_SYS_RESOURCE_H 1
#define HAVE_SYS_SELECT_H 1
#define HAVE_SYS_SENDFILE_H 1
#define HAVE_SYS_SOCKET_H 1
#define HAVE_SYS_STATFS_H 1
#define HAVE_SYS_STAT_H 1
#define HAVE_SYS_SYSCALL_H 1
#define HAVE_SYS_TIME_H 1
#define HAVE_SYS_TYPES_H 1
#define HAVE_SYS_UIO_H 1
#define HAVE_SYS_UN_H 1
#define HAVE_SYS_USER_H 1
#define HAVE_SYS_UTIME_H 1
#define HAVE_SYS_UTSNAME_H 1
#define HAVE_SYS_WAIT_H 1
#define HAVE_TERMIOS_H 1
#define HAVE_UCONTEXT_H 1
#define HAVE_UNISTD_H 1
#define HAVE_UNWIND_H 1
#define HAVE_USR_INCLUDE_MALLOC_H 1
#define HAVE_UTIME_H 1
#define HAVE_WCHAR_H 1
#define STDC_HEADERS 1
#define HAVE_SYS_ENDIAN_H 1
#define HAVE_COMPLEX_H 1

// QNX version of setlocale has limited functionality (compared to Linux) and doesn't accept NULL as an argument:
// https://www.qnx.com/developers/docs/7.1/#com.qnx.doc.neutrino.lib_ref/topic/s/setlocale.html
// https://www.qnx.com/developers/docs/8.0/com.qnx.doc.neutrino.lib_ref/topic/s/setlocale.html
#define SETLOCALE_NULL_NOT_SUPPORTED 1

#define DO_NOT_CALCULATE_DST_FOR_GMT_AND_UTC 1
