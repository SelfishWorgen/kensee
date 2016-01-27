// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the PARSER_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// PARSER_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef PARSER_EXPORTS
#define PARSER_API __declspec(dllexport)
#else
#define PARSER_API __declspec(dllimport)
#endif

typedef void (__stdcall *CallBackFn)(const char* token, int start_offset, int end_offset, const char*tag1, const char*tag2, const char*tag3); 

PARSER_API void  __stdcall ParseText(char *text, char *dataPath, CallBackFn callBackFn);
PARSER_API void  __stdcall  InitializeParser(char *dataPath);
PARSER_API void __stdcall FinalyzeParser();


