﻿Option Strict On

Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Security.AccessControl
Imports Microsoft.Win32
Imports System.Security.Principal

Namespace Win32
	Namespace ACL
		Public Module ACL

#Region "Consts"
			Private ReadOnly HKEY_CLASSES_ROOT As IntPtr = New IntPtr(-2147483648)
			Private ReadOnly HKEY_CURRENT_USER As IntPtr = New IntPtr(-2147483647)
			Private ReadOnly HKEY_LOCAL_MACHINE As IntPtr = New IntPtr(-2147483646)
			Private ReadOnly HKEY_USERS As IntPtr = New IntPtr(-2147483645)
			Private ReadOnly HKEY_PERFORMANCE_DATA As IntPtr = New IntPtr(-2147483644)
			Private ReadOnly HKEY_CURRENT_CONFIG As IntPtr = New IntPtr(-2147483643)
			Private ReadOnly HKEY_DYN_DATA As IntPtr = New IntPtr(-2147483642)

			''' <remarks>https://technet.microsoft.com/en-us/library/dd349804%28v=ws.10%29.aspx</remarks>
			Public Class SE
				''' <summary>Allows a parent process to replace the access token that is associated with a child process.</summary>
				Public Const ASSIGNPRIMARYTOKEN_NAME As String = "SeAssignPrimaryTokenPrivilege"

				''' <summary>Allows a process to generate entries in the security log.
				''' The security log is used to trace unauthorized system access. (See also "Manage auditing and security log" in this table.)</summary>
				Public Const AUDIT_NAME As String = "SeAuditPrivilege"

				''' <summary>Allows the user to circumvent file and directory permissions to back up the system.
				''' The privilege is selected only when an application attempts access through the NTFS backup application programming interface (API).
				''' Otherwise, normal file and directory permissions apply.
				''' 
				''' By default, this privilege is assigned to Administrators and Backup Operators. (See also "Restore files and directories" in this table.) </summary>
				Public Const BACKUP_NAME As String = "SeBackupPrivilege"

				''' <summary>Allows the user to pass through folders to which the user otherwise has no access while navigating an object path
				''' in any Microsoft® Windows® file system or in the registry. This privilege does not allow the user to list the contents of a folder
				''' it allows the user only to traverse its directories.
				''' By default, this privilege is assigned to Administrators, Backup Operators, Power Users, Users, and Everyone.</summary>
				Public Const CHANGE_NOTIFY_NAME As String = "SeChangeNotifyPrivilege"

				''' <summary> Required to create named file mapping objects in the global namespace during Terminal Services sessions.
				''' This privilege is enabled by default for administrators, services, and the local system account.
				''' User Right: Create global objects. Windows XP/2000: This privilege is not supported.
				''' 
				''' Note that this value is supported starting with Windows Server 2003, Windows XP with SP2, and Windows 2000 with SP4.</summary>
				Public Const CREATE_GLOBAL_NAME As String = "SeCreateGlobalPrivilege"

				''' <summary>Allows the user to create and change the size of a pagefile. 
				''' This is done by specifying a paging file size for a particular drive under Performance Options on the Advanced tab of System Properties.
				''' 
				''' By default, this privilege is assigned to Administrators. </summary>
				Public Const CREATE_PAGEFILE_NAME As String = "SeCreatePagefilePrivilege"

				''' <summary>Allows a process to create a directory object in the Windows 2000 object manager.
				''' This privilege is useful to kernel-mode components that extend the Windows 2000 object namespace.
				''' 
				''' Components that are running in kernel mode already have this privilege assigned to them;
				''' it is not necessary to assign them the privilege.</summary>
				Public Const CREATE_PERMANENT_NAME As String = "SeCreatePermanentPrivilege"

				''' <summary>Allows a process to create an access token by calling NtCreateToken() or other token-creating APIs.
				''' When a process requires this privilege, use the LocalSystem account (which already includes the privilege),
				''' rather than create a separate user account and assign this privilege to it. </summary>
				Public Const CREATE_TOKEN_NAME As String = "SeCreateTokenPrivilege"

				''' <summary>Allows the user to attach a debugger to any process.
				''' This privilege provides access to sensitive and critical operating system components.
				''' 
				''' By default, this privilege is assigned to Administrators. </summary>
				Public Const DEBUG_NAME As String = "SeDebugPrivilege"

				''' <summary>Allows the user to change the Trusted for Delegation setting on a user or computer object in Active Directory. 
				''' The user or computer that is granted this privilege must also have write access to the account control flags on the object.
				''' 
				''' Delegation of authentication is a capability that is used by multi-tier client/server applications. 
				''' It allows a front-end service to use the credentials of a client in authenticating to a back-end service. 
				''' For this to be possible, both client and server must be running under accounts that are trusted for delegation.
				''' 
				''' Misuse of this privilege or the Trusted for Delegation settings can make the network vulnerable to sophisticated attacks on the system
				''' that use Trojan horse programs, which impersonate incoming clients and use their credentials to gain access to network resources. </summary>
				Public Const ENABLE_DELEGATION_NAME As String = "SeEnableDelegationPrivilege"

				''' <summary>Required to impersonate. 
				''' User Right: Impersonate a client after authentication. Windows XP/2000: This privilege is not supported.
				''' 
				''' Note that this value is supported starting with Windows Server 2003, Windows XP with SP2, and Windows 2000 with SP4.</summary>
				Public Const IMPERSONATE_NAME As String = "SeImpersonatePrivilege"

				''' <summary>Allows a process that has Write Property access to another process to increase the processor quota that is assigned to the other process.
				''' This privilege is useful for system tuning, but it can be abused, as in a denial-of-service attack.
				''' 
				''' By default, this privilege is assigned to Administrators. </summary>
				Public Const INCREAQUOTA_NAME As String = "SeIncreaseQuotaPrivilege"

				''' <summary>Allows a process that has Write Property access to another process to increase the execution priority of the other process.
				''' A user with this privilege can change the scheduling priority of a process in the Task Manager dialog box.
				''' 
				''' By default, this privilege is assigned to Administrators. </summary>
				Public Const INC_BAPRIORITY_NAME As String = "SeIncreaseBasePriorityPrivilege"

				''' <summary>Allows a user to install and uninstall Plug and Play device drivers. 
				''' This privilege does not apply to device drivers that are not Plug and Play; 
				''' these device drivers can be installed only by Administrators. 
				''' 
				''' Note that device drivers run as trusted (highly privileged) programs; 
				''' a user can abuse this privilege by installing hostile programs and giving them destructive access to resources.
				''' 
				''' By default, this privilege is assigned to Administrators. </summary>
				Public Const LOAD_DRIVER_NAME As String = "SeLoadDriverPrivilege"

				''' <summary>Allows a process to keep data in physical memory, which prevents the system from paging the data to virtual memory on disk.
				''' Assigning this privilege can result in significant degradation of system performance. 
				''' 
				''' This privilege is obsolete and is therefore never selected.</summary>
				Public Const LOCK_MEMORY_NAME As String = "SeLockMemoryPrivilege"

				''' <summary>Allows the user to add a computer to a specific domain. 
				''' For the privilege to be effective, it must be assigned to the user as part of local security policy for domain controllers in the domain.
				''' A user who has this privilege can add up to 10 workstations to the domain.
				'''
				''' In Windows 2000, the behavior of this privilege is duplicated by the Create Computer Objects permission for organizational units
				''' and the default Computers container in Active DirectorySUP>™ Users who have the Create Computer Objects permission can add an
				''' unlimited number of computers to the domain. </summary>
				Public Const MACHINE_ACCOUNT_NAME As String = "SeMachineAccountPrivilege"

				''' <summary>Required to enable volume management privileges.
				''' User Right: Manage the files on a volume. </summary>
				Public Const MANAGE_VOLUME_NAME As String = "SeManageVolumePrivilege"

				''' <summary>Allows a user to run Microsoft® Windows NT® and Windows 2000 performance-monitoring tools to monitor the performance of nonsystem processes.
				''' By default, this privilege is assigned to Administrators and Power Users. </summary>
				Public Const PROF_SINGLE_PROCESS_NAME As String = "SeProfileSingleProcessPrivilege"

				''' <summary>Allows a user to shut down a computer from a remote location on the network. (See also "Shut down the system" in this table.)
				''' By default, this privilege is assigned to Administrators. </summary>
				Public Const REMOTE_SHUTDOWN_NAME As String = "SeRemoteShutdownPrivilege"

				''' <summary>Allows a user to circumvent file and directory permissions when restoring backed-up files and directories
				''' and to set any valid security principal as the owner of an object. (See also "Back up files and directories" in this table.)
				''' By default, this privilege is assigned to Administrators and Backup Operators. </summary>
				Public Const RESTORE_NAME As String = "SeRestorePrivilege"

				''' <summary>Allows a user to specify object access auditing options for individual resources such as files,
				''' Active Directory objects, and registry keys. Object access auditing is not actually performed unless you have enabled
				''' it in Audit Policy (under Security Settings , Local Policies ). 
				''' 
				''' A user who has this privilege also can view and clear the security log from Event Viewer.
				''' By default, this privilege is assigned to Administrators.</summary>
				Public Const SECURITY_NAME As String = "SeSecurityPrivilege"

				''' <summary>Allows a user to shut down the local computer. (See also "Force shutdown from a remote system" in this table.)
				''' 
				''' In Microsoft® Windows® 2000 Professional, this privilege is assigned by default to Administrators, Backup Operators, Power Users, and Users.
				''' In Microsoft® Windows® 2000 Server, this privilege is by default not assigned to Users; it is assigned only to Administrators, Backup Operators, and Power Users.</summary>
				Public Const SHUTDOWN_NAME As String = "SeShutdownPrivilege"

				''' <summary>Allows the user to set the time for the internal clock of the computer.
				''' By default, this privilege is assigned to Administrators and Power Users. </summary>
				Public Const SYSTEMTIME_NAME As String = "SeSystemtimePrivilege"

				''' <summary>Allows modification of system environment variables either by a process through an API or by a user through System Properties .
				''' By default, this privilege is assigned to Administrators. </summary>
				Public Const SYSTEM_ENVIRONMENT_NAME As String = "SeSystemEnvironmentPrivilege"

				''' <summary>Allows a user to run Windows NT and Windows 2000 performance-monitoring tools to monitor the performance of system processes.
				''' By default, this privilege is assigned to Administrators. </summary>
				Public Const SYSTEM_PROFILE_NAME As String = "SeSystemProfilePrivilege"

				''' <summary>Allows a user to take ownership of any securable object in the system, 
				''' including Active Directory objects, files and folders, printers, registry keys, processes, and threads.
				''' By default, this privilege is assigned to Administrators.</summary>
				Public Const TAKE_OWNERSHIP_NAME As String = "SeTakeOwnershipPrivilege"

				''' <summary>Allows a process to authenticate like a user and thus gain access to the same resources as a user.
				''' Only low-level authentication services should require this privilege.
				''' 
				''' Note that potential access is not limited to what is associated with the user by default;
				''' the calling process might request that arbitrary additional privileges be added to the access token.
				''' Note that the calling process can also build an anonymous token that does not provide a primary identity for tracking events in the audit log.
				'''
				''' When a service requires this privilege, configure the service to use the LocalSystem account (which already includes the privilege),
				''' rather than create a separate account and assign the privilege to it.</summary>
				Public Const TCB_NAME As String = "SeTcbPrivilege"

				''' <summary>Allows the user of a portable computer to undock the computer by clicking Eject PC on the Start menu.
				''' By default, this privilege is assigned to Administrators, Power Users, and Users. </summary>
				Public Const UNDOCK_NAME As String = "SeUndockPrivilege"

				''' <summary>Required to read unsolicited input from a terminal device.
				''' User Right: Not applicable. </summary>
				Public Const UNSOLICITED_INPUT_NAME As String = "SeUnsolicitedInputPrivilege"
			End Class
#End Region

#Region "Enums"

			Private Enum SID_NAME_USE As Integer
				SidTypeUser = 1
				SidTypeGroup
				SidTypeDomain
				SidTypeAlias
				SidTypeWellKnownGroup
				SidTypeDeletedAccount
				SidTypeInvalid
				SidTypeUnknown
				SidTypeComputer
			End Enum

			Private Enum SE_OBJECT_TYPE As UInt32
				''' <summary>Unknown object type.</summary>
				SE_UNKNOWN_OBJECT_TYPE

				''' <summary>Indicates a file or directory. 
				''' The name string that identifies a file or directory object can be in one of the following formats:
				''' 
				''' - A relative path, such as FileName.dat or ..\FileName
				''' - An absolute path, such as FileName.dat, C:\DirectoryName\FileName.dat, or G:\RemoteDirectoryName\FileName.dat.
				''' - A UNC name, such as \\ComputerName\ShareName\FileName.dat.</summary>
				SE_FILE_OBJECT

				''' <summary>Indicates a Windows service. 
				''' A service object can be a local service, such as ServiceName, or a remote service, such as \\ComputerName\ServiceName.</summary>
				SE_SERVICE

				''' <summary>Indicates a printer.
				''' A printer object can be a local printer, such as PrinterName, or a remote printer, such as \\ComputerName\PrinterName.</summary>
				SE_PRINTER

				''' <summary>Indicates a registry key. 
				''' A registry key object can be in the local registry, such as CLASSES_ROOT\SomePath or in a remote registry,
				''' such as \\ComputerName\CLASSES_ROOT\SomePath.
				''' 
				''' The names of registry keys must use the following literal strings to identify the predefined registry keys:
				''' "CLASSES_ROOT", "CURRENT_USER", "MACHINE", and "USERS".</summary>
				SE_REGISTRY_KEY

				''' <summary>Indicates a network share.
				''' A share object can be local, such as ShareName, or remote, such as \\ComputerName\ShareName.</summary>
				SE_LMSHARE

				''' <summary>Indicates a local kernel object.
				''' The GetSecurityInfo and SetSecurityInfo functions support all types of kernel objects. 
				''' The GetNamedSecurityInfo and SetNamedSecurityInfo functions work only with the following kernel objects: 
				''' semaphore, event, mutex, waitable timer, and file mapping.</summary>
				SE_KERNEL_OBJECT

				''' <summary>Indicates a window station or desktop object on the local computer.
				''' You cannot use GetNamedSecurityInfo and SetNamedSecurityInfo with these objects
				''' because the names of window stations or desktops are not unique.</summary>
				SE_WINDOW_OBJECT

				''' <summary>Indicates a directory service object or a property set or property of a directory service object.
				''' The name string for a directory service object must be in X.500 form, for example:
				'''
				''' CN=SomeObject,OU=ou2,OU=ou1,DC=DomainName,DC=CompanyName,DC=com,O=internet</summary>
				SE_DS_OBJECT

				''' <summary>Indicates a directory service object and all of its property sets and properties. </summary>
				SE_DS_OBJECT_ALL

				''' <summary>Indicates a provider-defined object.</summary>
				SE_PROVIDER_DEFINED_OBJECT

				''' <summary>Indicates a WMI object.</summary>
				SE_WMIGUID_OBJECT

				''' <summary>Indicates an object for a registry entry under WOW64. </summary>
				SE_REGISTRY_WOW64_32KEY
			End Enum

			<Flags()>
			Private Enum SECURITY_INFORMATION As UInt32
				''' <summary>The owner identifier of the object is being referenced.</summary>
				OWNER_SECURITY_INFORMATION = &H1UI

				''' <summary>The primary group identifier of the object is being referenced.</summary>
				GROUP_SECURITY_INFORMATION = &H2UI

				''' <summary>The DACL of the object is being referenced.</summary>
				DACL_SECURITY_INFORMATION = &H4UI

				''' <summary>The SACL of the object is being referenced.</summary>
				SACL_SECURITY_INFORMATION = &H8UI

				''' <summary>The mandatory integrity label is being referenced.</summary>
				LABEL_SECURITY_INFORMATION = &H10UI

				''' <summary>The SACL inherits access control entries (ACEs) from the parent object.</summary>
				UNPROTECTED_SACL_SECURITY_INFORMATION

				''' <summary>The DACL inherits ACEs from the parent object.</summary>
				UNPROTECTED_DACL_SECURITY_INFORMATION

				''' <summary>The SACL cannot inherit ACEs.</summary>
				PROTECTED_SACL_SECURITY_INFORMATION

				''' <summary>The DACL cannot inherit ACEs.</summary>
				PROTECTED_DACL_SECURITY_INFORMATION

				''' <summary>A SYSTEM_RESOURCE_ATTRIBUTE_ACE (section 2.4.4.15) is being referenced.</summary>
				''' <remarks>https://msdn.microsoft.com/en-us/library/hh877837.aspx</remarks>
				ATTRIBUTE_SECURITY_INFORMATION

				''' <summary>A SYSTEM_SCOPED_POLICY_ID_ACE (section 2.4.4.16) is being referenced.</summary>
				''' <remarks>https://msdn.microsoft.com/en-us/library/hh877846.aspx</remarks>
				SCOPE_SECURITY_INFORMATION

				''' <summary>The security descriptor is being accessed for use in a backup operation.</summary>
				BACKUP_SECURITY_INFORMATION
			End Enum

			<Flags()>
			Private Enum REGSAM As UInt32
				''' <summary>Permission to query subkey data.</summary>
				KEY_QUERY_VALUE = &H1UI

				''' <summary>Permission to set subkey data.</summary>
				KEY_SET_VALUE = &H2UI

				''' <summary>Permission to create subkeys.
				''' Subkeys directly underneath the 'HKEY_LOCAL_MACHINE' and 'HKEY_USERS'
				''' predefined keys cannot be created even if this bit is set.</summary>
				KEY_CREATE_SUB_KEY = &H4UI

				''' <summary>Permission to enumerate subkeys.</summary>
				KEY_ENUMERATE_SUB_KEYS = &H8UI

				''' <summary>Permission to create a symbolic link.</summary>
				KEY_CREATE_LINK = &H20UI

				''' <summary>When set, indicates that a registry server on a 64-bit operating system operates on the 64-bit key namespace.</summary>
				KEY_WOW64_64KEY = &H100UI

				''' <summary>When set, indicates that a registry server on a 64-bit operating system operates on the 32-bit key namespace.</summary>
				KEY_WOW64_32KEY = &H200UI

				''' <summary>Permission for read access.</summary>
				KEY_EXECUTE = &H20019UI

				''' <summary>Permission for change notification.</summary>
				KEY_NOTIFY = &H10UI

				''' <summary>Combination of KEY_QUERY_VALUE, KEY_ENUMERATE_SUB_KEYS, and KEY_NOTIFY access.</summary>
				KEY_READ = &H20019UI

				''' <summary>Combination of KEY_SET_VALUE and KEY_CREATE_SUB_KEY access.</summary>
				KEY_WRITE = &H20006UI

				''' <summary>Combination of KEY_QUERY_VALUE, KEY_ENUMERATE_SUB_KEYS, KEY_NOTIFY, KEY_CREATE_SUB_KEY, KEY_CREATE_LINK, and KEY_SET_VALUE access.</summary>
				KEY_ALL_ACCESS = &H2F003FUI
			End Enum

			<Flags()>
			Private Enum REG_OPTION As UInt32
				''' <summary>This key is not volatile; this is the default.
				''' The information is stored in a file and is preserved when the system is restarted.
				''' The RegSaveKey function saves keys that are not volatile.</summary>
				NON_VOLATILE = &H0UI

				''' <summary>All keys created by the function are volatile.
				''' The information is stored in memory and is not preserved when the corresponding registry hive is unloaded.
				''' For HKEY_LOCAL_MACHINE, this occurs only when the system initiates a full shutdown.
				''' For registry keys loaded by the RegLoadKey function, this occurs when the corresponding RegUnLoadKey is performed. 
				''' The RegSaveKey function does not save volatile keys. This flag is ignored for keys that already exist. 
				''' 
				''' Note: On a user selected shutdown, a fast startup shutdown is the default behavior for the system.</summary>
				VOLATILE = &H1UI

				''' <summary>This key is a symbolic link. 
				''' The target path is assigned to the L"SymbolicLinkValue" value of the key.
				''' The target path must be an absolute registry path.
				''' 
				''' Note: Registry symbolic links should only be used for for application compatibility when absolutely necessary.</summary>
				CREATE_LINK = &H2UI

				''' <summary>If this flag is set, the function ignores the samDesired parameter and attempts to open the key with the access required to backup or restore the key. 
				''' If the calling thread has the SE_BACKUP_NAME privilege enabled, 
				''' the key is opened with the ACCESS_SYSTEM_SECURITY and KEY_READ access rights.
				''' 
				''' If the calling thread has the SE_RESTORE_NAME privilege enabled, beginning with Windows Vista, 
				''' the key is opened with the ACCESS_SYSTEM_SECURITY, DELETE and KEY_WRITE access rights. 
				''' 
				''' If both privileges are enabled, the key has the combined access rights for both privileges. 
				''' For more information, see Running with Special Privileges.</summary>
				''' <remarks>https://msdn.microsoft.com/en-us/library/windows/desktop/ms717802(v=vs.85).aspx</remarks>
				BACKUP_RESTORE = &H4UI
			End Enum

			<Flags()>
			Private Enum TOKENS As UInt32
				READ_CONTROL = &H20000UI

				STANDARD_RIGHTS_READ = READ_CONTROL
				STANDARD_RIGHTS_WRITE = READ_CONTROL
				STANDARD_RIGHTS_EXECUTE = READ_CONTROL
				STANDARD_RIGHTS_REQUIRED = &HF0000UI

				''' <summary>Required to change the default owner, primary group, or DACL of an access token.</summary>
				ADJUST_DEFAULT = &H80UI

				''' <summary>Required to adjust the attributes of the groups in an access token.</summary>
				ADJUST_GROUPS = &H40UI

				''' <summary>Required to enable or disable the privileges in an access token.</summary>
				ADJUST_PRIVILEGES = &H20UI

				''' <summary>Required to adjust the session ID of an access token.
				''' The SE_TCB_NAME privilege is required.</summary>
				ADJUST_SESSIONID = &H100UI

				''' <summary>Required to attach a primary token to a process. 
				''' The SE_ASSIGNPRIMARYTOKEN_NAME privilege is also required to accomplish this task.</summary>
				ASSIGN_PRIMARY = &H1UI

				''' <summary>Required to duplicate an access token.</summary>
				DUPLICATE = &H2UI

				''' <summary>Combines STANDARD_RIGHTS_EXECUTE and TOKEN_IMPERSONATE.</summary>
				EXECUTE = STANDARD_RIGHTS_EXECUTE Or IMPERSONATE

				''' <summary>Required to attach an impersonation access token to a process.</summary>
				IMPERSONATE = &H4UI

				''' <summary>Required to query an access token.</summary>
				QUERY = &H8UI

				''' <summary>Required to query the source of an access token.</summary>
				QUERY_SOURCE = &H10UI

				''' <summary>Combines STANDARD_RIGHTS_READ and TOKEN_QUERY.</summary>
				READ = STANDARD_RIGHTS_READ Or QUERY

				''' <summary>Combines STANDARD_RIGHTS_WRITE, TOKEN_ADJUST_PRIVILEGES, TOKEN_ADJUST_GROUPS, and TOKEN_ADJUST_DEFAULT.</summary>
				WRITE = STANDARD_RIGHTS_WRITE Or ADJUST_PRIVILEGES Or ADJUST_GROUPS Or ADJUST_DEFAULT

				''' <summary>Combines all possible access rights for a token.</summary>
				ALL_ACCESS_P = STANDARD_RIGHTS_REQUIRED Or ASSIGN_PRIMARY Or DUPLICATE Or IMPERSONATE Or QUERY Or QUERY_SOURCE Or ADJUST_PRIVILEGES Or ADJUST_GROUPS Or ADJUST_DEFAULT

				''' <summary>Combines all possible access rights for a token.</summary>
				ALL_ACCESS = ALL_ACCESS_P Or ADJUST_SESSIONID
			End Enum

			<Flags()>
			Private Enum SE_PRIVILEGE As UInt32
				''' <summary></summary>
				ENABLED_BY_DEFAULT = &H1UI

				''' <summary>The function enables the privilege.</summary>
				ENABLED = &H2UI

				''' <summary>The privilege is removed from the list of privileges in the token.
				''' The other privileges in the list are reordered to remain contiguous.
				''' 
				''' SE_PRIVILEGE_REMOVED supersedes SE_PRIVILEGE_ENABLED.</summary>
				REMOVED = &H4UI

				''' <summary></summary>
				USED_FOR_ACCESS = &H80000000UI
			End Enum

#End Region

#Region "Structures"

			<StructLayout(LayoutKind.Sequential)>
			Private Structure TOKEN_PRIVILEGES
				Public PrivilegeCount As UInt32
				<MarshalAs(UnmanagedType.ByValArray, SizeConst:=1)>
				Public Privileges() As LUID_AND_ATTRIBUTES
			End Structure

			<StructLayout(LayoutKind.Sequential)>
			Private Structure LUID_AND_ATTRIBUTES
				Public Luid As LUID
				Public Attributes As UInt32
			End Structure

			<StructLayout(LayoutKind.Sequential)>
			Private Structure LUID
				Public LowPart As UInt32
				Public HighPart As UInt32
			End Structure
#End Region

#Region "P/Invoke"
			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function AdjustTokenPrivileges(
   <[In]()> ByVal TokenHandle As IntPtr,
   <[In]()> ByVal DisableAllPrivileges As Boolean,
   <[In](), [Optional]()> ByVal NewState As IntPtr,
   <[In](), [Optional]()> ByVal BufferLength As UInt32,
   <[Out](), [Optional]()> ByVal PreviousState As IntPtr,
   <[Out](), [Optional]()> ByRef ReturnLength As UInt32) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function OpenProcessToken(
   <[In]()> ByVal ProcessHandle As IntPtr,
   <[In]()> ByVal DesiredAccess As UInt32,
   <[Out]()> ByRef TokenHandle As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function

			<DllImport("kernel32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function GetCurrentProcess() As IntPtr
			End Function

			<DllImport("Advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function LookupPrivilegeValue(
   <[In](), [Optional](), MarshalAs(UnmanagedType.LPWStr)> ByVal lpSystemName As String,
   <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal lpName As String,
   <[Out]()> ByRef lpLuid As LUID) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function


			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function RegGetValue(
   <[In]()> ByVal hKey As IntPtr,
   <[In](), [Optional](), MarshalAs(UnmanagedType.LPWStr)> ByVal lpSubKey As String,
   <[In](), [Optional](), MarshalAs(UnmanagedType.LPWStr)> ByVal lpValue As String,
   <[In](), [Optional]()> ByVal dwFlags As UInt32,
   <[Out](), [Optional]()> ByRef pdwType As UInt32,
   <[Out](), [Optional]()> ByVal pvData As IntPtr,
   <[In](), [Out](), [Optional]()> ByRef pcbData As UInt32) As UInt32
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function RegGetKeySecurity(
   <[In]()> ByVal hKey As IntPtr,
   <[In]()> ByVal SecurityInformation As SECURITY_INFORMATION,
   <[In](), [Out](), [Optional]()> ByRef pSecurityDescriptor As IntPtr,
   <[In](), [Out]()> ByRef lpcbSecurityDescriptor As UInt32) As UInt32
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function GetSecurityDescriptorOwner(
   <[In]()> ByRef pSecurityDescriptor As IntPtr,
   <[In](), [Out]()> ByRef pOwner As IntPtr,
   <[Out]()> ByRef lpbOwnerDefaulted As Integer) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function IsValidSecurityDescriptor(
   <[In]()> ByVal pSecurityDescriptor As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function InitializeSecurityDescriptor(
   <[Out]()> ByVal pSecurityDescriptor As IntPtr,
   <[In]()> ByVal dwRevision As UInt32) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function RegOpenKeyEx(
   <[In]()> ByVal hKey As IntPtr,
   <[In](), [Optional](), MarshalAs(UnmanagedType.LPWStr)> ByVal subKey As String,
   <[In]()> ByVal ulOptions As UInt32,
   <[In]()> ByVal samDesired As REGSAM,
   <[Out]()> ByRef phkResult As IntPtr) As UInt32
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function RegCloseKey(
 <[In]()> ByVal hKey As IntPtr) As UInt32
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function RegCreateKeyEx(
   <[In]()> ByVal hKey As IntPtr,
   <[In]()> lpSubKey As String,
   ByVal Reserved As Integer,
   <[In](), [Optional](), MarshalAs(UnmanagedType.LPWStr)> lpClass As String,
   <[In]()> dwOptions As REG_OPTION,
   <[In]()> samDesired As REGSAM,
   <[In](), [Optional]()> lpSecurityAttributes As IntPtr,
   <[Out]()> ByRef hkResult As IntPtr,
   <[Out](), [Optional]()> ByRef lpdwDisposition As UInt32) As UInt32
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function GetSecurityInfo(
   <[In]()> ByVal handle As IntPtr,
   <[In]()> ByVal objectType As SE_OBJECT_TYPE,
   <[In]()> ByVal securityInfo As SECURITY_INFORMATION,
   <[Out](), [Optional]()> ByRef sidOwner As IntPtr,
   <[Out](), [Optional]()> ByRef sidGroup As IntPtr,
   <[Out](), [Optional]()> ByRef dacl As IntPtr,
   <[Out](), [Optional]()> ByRef sacl As IntPtr,
   <[Out](), [Optional]()> ByRef securityDescriptor As IntPtr) As UInt32
			End Function

			<DllImport("advapi32", CharSet:=CharSet.Auto, SetLastError:=True)>
			Private Function ConvertSidToStringSid(
   <[In]()> ByRef Sid As IntPtr,
   <[Out](), MarshalAs(UnmanagedType.LPTStr)> ByRef StringSid As String) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function



			<DllImport("kernel32.dll", SetLastError:=True)>
			Private Function LocalFree(
   <[In]()> ByVal handle As IntPtr) As IntPtr
			End Function

			<DllImport("kernel32.dll", SetLastError:=True)>
			Private Function CloseHandle(
   <[In]()> ByVal hHandle As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function InitializeSid(
   <[Out]()> ByRef Sid As IntPtr,
   <[In]()> ByVal pIdentifierAuthority As IntPtr,
   <[In]()> ByVal nSubAuthorityCount As Byte) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function

			'BOOL WINAPI ConvertSecurityDescriptorToStringSecurityDescriptor(
			'  _In_  PSECURITY_DESCRIPTOR SecurityDescriptor,
			'  _In_  DWORD                RequestedStringSDRevision,
			'  _In_  SECURITY_INFORMATION SecurityInformation,
			'  _Out_ LPTSTR               *StringSecurityDescriptor,
			'  _Out_ PULONG               StringSecurityDescriptorLen
			');

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function GetSecurityDescriptorControl(
   <[In]()> ByRef pSecurityDescriptor As IntPtr,
   <[Out]()> ByRef pControl As IntPtr,
   <[Out]()> ByRef lpdwRevision As UInt32) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function

			<DllImport("advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
			Private Function ConvertSecurityDescriptorToStringSecurityDescriptor(
   <[In]()> ByRef SecurityDescriptor As IntPtr,
   <[In]()> ByVal RequestedStringSDRevision As UInt32,
   <[In]()> ByVal SecurityInformation As SECURITY_INFORMATION,
   <[Out](), MarshalAs(UnmanagedType.LPWStr)> ByRef StringSecurityDescriptor As String,
   <[Out]()> ByRef StringSecurityDescriptorLen As UInt32) As <MarshalAs(UnmanagedType.Bool)> Boolean
			End Function
#End Region

			Public Sub AddPriviliges(ByVal ParamArray priviliges() As String)
				AdjustToken(True, GetCurrentProcess(), priviliges)
			End Sub

			Public Sub RemovePriviliges(ByVal ParamArray priviliges() As String)
				AdjustToken(False, GetCurrentProcess(), priviliges)
			End Sub

			Private Sub AdjustToken(ByVal enable As Boolean, ByVal ptrProcess As IntPtr, ByVal ParamArray priviliges() As String)
				Dim ptrToken As IntPtr = IntPtr.Zero

				Try
					Dim success As Boolean = OpenProcessToken(ptrProcess, TOKENS.ADJUST_PRIVILEGES Or TOKENS.QUERY, ptrToken)

					If Not success Then
						Throw New Win32Exception()
					End If

					Dim luid As LUID
					Dim luidAndAttributes As New List(Of LUID_AND_ATTRIBUTES)
					Dim requiredSize As UInt32

					For Each privilige In priviliges
						If Not LookupPrivilegeValue(Nothing, privilige, luid) Then
							Throw New Win32Exception()
						End If

						Using newState = New StructPtr(New TOKEN_PRIVILEGES With
						 {
						  .PrivilegeCount = 1,
						  .Privileges =
						  {
						   New LUID_AND_ATTRIBUTES() With
						   {
						 .Luid = luid,
						 .Attributes = If(enable, SE_PRIVILEGE.ENABLED, SE_PRIVILEGE.REMOVED)
						   }
						  }
						 })

							If Not AdjustTokenPrivileges(ptrToken, False, newState.Ptr, 0UI, IntPtr.Zero, requiredSize) Then
								Dim err As UInt32 = GetLastWin32ErrorU()

								If err <> Errors.INSUFFICIENT_BUFFER AndAlso err <> Errors.NOT_ALL_ASSIGNED Then
									Throw New Win32Exception(GetInt32(err))
								End If
							End If

						End Using

					Next

				Catch ex As Exception
					ShowException(ex)
				Finally
					If ptrToken <> IntPtr.Zero Then
						CloseHandle(ptrToken)
					End If
				End Try
			End Sub

			' Adds an ACL entry on the specified directory for the specified account.
			Public Sub AddDirectorySecurity(ByVal path As String, ByVal Rights As FileSystemRights, ByVal ControlType As AccessControlType)
				' Create a new DirectoryInfoobject.
				Dim dInfo As New DirectoryInfo(path)

				Dim sid = New SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, Nothing)
				' Get a DirectorySecurity object that represents the 
				' current security settings.
				'Dim dSecurity As DirectorySecurity = dInfo.GetAccessControl()
				'Activate necessary admin privileges to make changes without NTFS perms
				ACl.AddPriviliges(ACl.SE.SECURITY_NAME, ACl.SE.BACKUP_NAME, ACl.SE.RESTORE_NAME, ACl.SE.TAKE_OWNERSHIP_NAME)

				'Create a new acl from scratch.
				Dim newacl As New System.Security.AccessControl.DirectorySecurity()
				'set owner only here (needed for WinXP)
				newacl.SetOwner(sid)
				dInfo.SetAccessControl(newacl)
				'This remove inheritance.
				newacl.SetAccessRuleProtection(True, False)

				' Add the FileSystemAccessRule to the security settings. 
				newacl.AddAccessRule(New FileSystemAccessRule(sid, Rights, ControlType))

				sid = New SecurityIdentifier(WellKnownSidType.LocalSystemSid, Nothing)
				newacl.AddAccessRule(New FileSystemAccessRule(sid, Rights, ControlType))

				sid = New SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, Nothing)
				newacl.AddAccessRule(New FileSystemAccessRule(sid, Rights, ControlType))

				' Set the new access settings.
				dInfo.SetAccessControl(newacl)
			End Sub

			Public Sub Addregistrysecurity(ByVal regkey As RegistryKey, ByVal subkeyname As String, ByVal Rights As RegistryRights, ByVal ControlType As AccessControlType)
				Dim subkey As RegistryKey
				Dim rs As New RegistrySecurity()
				Dim sid = New SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, Nothing)

				ACl.AddPriviliges(ACl.SE.SECURITY_NAME, ACl.SE.BACKUP_NAME, ACl.SE.RESTORE_NAME, ACl.SE.TAKE_OWNERSHIP_NAME)

				'Dim originalsid = regkey.OpenSubKey(subkeyname, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions).GetAccessControl.GetOwner(GetType(System.Security.Principal.SecurityIdentifier))
				'MsgBox(originalsid.ToString)
				subkey = regkey.OpenSubKey(subkeyname, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership)
				rs.SetOwner(sid)

				' Set the new access settings.Owner
				subkey.SetAccessControl(rs)
				rs.SetAccessRuleProtection(True, False)

				'rs.AddAccessRule(New RegistryAccessRule(sid, Rights, ControlType))
				sid = New SecurityIdentifier(WellKnownSidType.LocalSystemSid, Nothing)
				rs.AddAccessRule(New RegistryAccessRule(sid, Rights, ControlType))

				'sid = New SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, Nothing)
				'rs.AddAccessRule(New RegistryAccessRule(sid, Rights, ControlType))
				' Set the new access settings.
				subkey.SetAccessControl(rs)
			End Sub

			' Removes an ACL entry on the specified directory for the specified account.
			Public Sub RemoveDirectorySecurity(ByVal FileName As String, ByVal Account As String, ByVal Rights As FileSystemRights, ByVal ControlType As AccessControlType)
				' Create a new DirectoryInfo object.
				Dim dInfo As New DirectoryInfo(FileName)

				' Get a DirectorySecurity object that represents the 
				' current security settings.
				Dim dSecurity As DirectorySecurity = dInfo.GetAccessControl()

				' Add the FileSystemAccessRule to the security settings. 
				dSecurity.RemoveAccessRule(New FileSystemAccessRule(Account, Rights, ControlType))

				' Set the new access settings.
				dInfo.SetAccessControl(dSecurity)

			End Sub


			Public Sub test3(ByVal debugOpt As Int32, Optional ByVal regKey As String = "SOFTWARE\ATI")
				'  Throw New Win32Exception(5)

				If debugOpt <> 0 Then
					AddPriviliges(SE.BACKUP_NAME, SE.RESTORE_NAME, SE.SECURITY_NAME, SE.TAKE_OWNERSHIP_NAME)
				End If

				Dim ptrRegKey As IntPtr = IntPtr.Zero
				Dim returnValue As UInt32

				Try
					returnValue = RegOpenKeyEx(HKEY_LOCAL_MACHINE, regKey, 0UI, REGSAM.KEY_READ, ptrRegKey)
					MsgBox("RegOpenKeyEx: " & returnValue.ToString())

					If returnValue <> 0UI Then
						Throw New Win32Exception(GetInt32(returnValue))
					End If

					Dim ptrOwner As IntPtr = IntPtr.Zero
					Dim ptrSecurity As IntPtr = IntPtr.Zero
					Dim requiredSize As UInt32 = 0UI

					returnValue = RegGetKeySecurity(ptrRegKey, SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION, ptrSecurity, requiredSize)

					If returnValue <> Errors.INSUFFICIENT_BUFFER Then
						Throw New Win32Exception(GetInt32(returnValue))
					End If

					ptrSecurity = Marshal.AllocHGlobal(GetInt32(requiredSize))
					returnValue = 0UI

					If Not InitializeSecurityDescriptor(ptrSecurity, 1) Then
						Throw New Win32Exception(GetLastWin32Error)
					End If

					returnValue = RegGetKeySecurity(ptrRegKey, SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION, ptrSecurity, requiredSize)

					If returnValue <> 0UI Then
						Throw New Win32Exception(GetInt32(returnValue))
					End If

					Dim rev As UInt32
					Dim ptrSecurityControl As IntPtr = IntPtr.Zero
					Dim getRev As Boolean = GetSecurityDescriptorControl(ptrSecurity, ptrSecurityControl, rev)
					Dim isValid As Boolean = IsValidSecurityDescriptor(ptrSecurity)

					Dim strOwner As String = Nothing
					Dim strOwnerLen As UInt32

					Dim success As Boolean = ConvertSecurityDescriptorToStringSecurityDescriptor(ptrSecurity, rev, SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION, strOwner, strOwnerLen)

					If Not success Then
						Throw New Win32Exception(GetLastWin32Error)
					End If

					'Dim defaulted As Int32
					'Dim success As Boolean = GetSecurityDescriptorOwner(ptrSecurity, ptrOwner, defaulted)

					'If Not success Then
					'    Throw New Win32Exception(GetLastWin32Error)
					'End If

					'If returnValue <> 0UI Then
					'    Throw New Win32Exception(GetInt32(returnValue))
					'End If

					'If ptrOwner <> IntPtr.Zero Then
					'    Dim ptrOwnerSid As IntPtr = IntPtr.Zero

					'    Try
					'        Dim sidStr As String = Nothing

					'        If ConvertSidToStringSid(ptrOwner, sidStr) Then
					'            MsgBox(sidStr)
					'        Else
					'            '   ERROR_INVALID_SID()
					'            '   1337 (0x539)
					'            '   The security ID structure is invalid.

					'            Throw New Win32Exception(GetLastWin32Error)
					'        End If
					'    Finally
					'        If ptrOwnerSid <> IntPtr.Zero Then
					'            LocalFree(ptrOwnerSid)
					'        End If
					'    End Try
					'End If
				Catch Ex As Exception
					ShowException(Ex)
				Finally
					If ptrRegKey <> IntPtr.Zero Then
						RegCloseKey(ptrRegKey)
					End If
				End Try
			End Sub

		End Module
	End Namespace
End Namespace