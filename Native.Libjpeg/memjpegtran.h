#include <stdio.h>
#include "jpeglib.h"
#include <setjmp.h>

struct memjpegtran_error_mgr
{
	struct jpeg_error_mgr pub;
	jmp_buf setjmp_buffer;
};

typedef struct memjpegtran_error_mgr * memjpegtran_error_ptr;
