/* Modified 2017 by Gurov Yury.
 *
 * This file was mutilated to perform in-memory lossless optimization only
 */

#include "cdjpeg.h"
#include "transupp.h"
#include "jversion.h"
#include "memjpegtran.h"

void memjpegtran_error_exit(j_common_ptr cinfo)
{
	/* cinfo->err really points to a my_error_mgr struct, so coerce pointer */
	memjpegtran_error_ptr err = (memjpegtran_error_ptr)cinfo->err;

	/* Always display the message. */
	/* We could postpone this until after returning, if we chose. */
	(*cinfo->err->output_message) (cinfo);

	/* Return control to the setjmp point */
	longjmp(err->setjmp_buffer, 1);
}

void prepare_options(
    j_compress_ptr cinfo,
    boolean optimize,
    boolean progressive,
    boolean arithmetic)
{
    cinfo->err->trace_level = 0;
    cinfo->arith_code = arithmetic;
    cinfo->optimize_coding = optimize;
    cinfo->mem->max_memory_to_use = 10L * 1000L * 1000L;
    if (progressive)
        jpeg_simple_progression(cinfo);
}


__declspec(dllexport) char OptimizeMemoryToMemory(
    unsigned char *inputbuffer,
    const long inputsize,
    unsigned char *outbuffer,
    unsigned long *outsize,
    char copyflag,
    char optimize,
    char progressive,
    char grayscale,
    char trim,
    char arithmetic)
{
    struct jpeg_decompress_struct srcinfo;
    struct jpeg_compress_struct dstinfo;
    struct memjpegtran_error_mgr jsrcerr, jdsterr;
    jvirt_barray_ptr * src_coef_arrays;
    jvirt_barray_ptr * dst_coef_arrays;
    unsigned char *_outarr = NULL;
    unsigned long _outsize = 0;
    JCOPY_OPTION copy = copyflag == FALSE ? JCOPYOPT_NONE : JCOPYOPT_ALL;
    jpeg_transform_info transformoption;
    transformoption.transform = JXFORM_NONE;
    transformoption.perfect = FALSE;
    transformoption.trim = trim;
    transformoption.force_grayscale = grayscale;
    transformoption.crop = FALSE;
    transformoption.crop_width_set = JCROP_UNSET;
    transformoption.crop_height_set = JCROP_UNSET;
    transformoption.crop_xoffset_set = JCROP_UNSET;
    transformoption.crop_yoffset_set = JCROP_UNSET; 
    
    srcinfo.err = jpeg_std_error(&jsrcerr.pub);
	jsrcerr.pub.error_exit = memjpegtran_error_exit;
    jpeg_create_decompress(&srcinfo);
	if (setjmp(jsrcerr.setjmp_buffer)) 
	{
		jpeg_destroy_compress(&dstinfo);
		jpeg_destroy_decompress(&srcinfo);

		if (_outsize > 0)
			free(_outarr);
		return FALSE;
	}
    dstinfo.err = jpeg_std_error(&jdsterr.pub);
	jdsterr.pub.error_exit = memjpegtran_error_exit;
    jpeg_create_compress(&dstinfo);
	if (setjmp(jdsterr.setjmp_buffer))
	{
		jpeg_destroy_compress(&dstinfo);
		jpeg_destroy_decompress(&srcinfo);

		if (_outsize > 0)
			free(_outarr);
		return FALSE;
	}
    
    prepare_options(&dstinfo, optimize, progressive, arithmetic);
    jsrcerr.pub.trace_level = jdsterr.pub.trace_level;
    srcinfo.mem->max_memory_to_use = dstinfo.mem->max_memory_to_use;
    jpeg_mem_src(&srcinfo, inputbuffer, inputsize);
    jcopy_markers_setup(&srcinfo, copy);

    (void)jpeg_read_header(&srcinfo, TRUE);

    if (!jtransform_request_workspace(&srcinfo, &transformoption))
    {
        return FALSE;
    }
    src_coef_arrays = jpeg_read_coefficients(&srcinfo);
    jpeg_copy_critical_parameters(&srcinfo, &dstinfo);

    dst_coef_arrays = jtransform_adjust_parameters(&srcinfo, &dstinfo,
        src_coef_arrays,
        &transformoption);

    prepare_options(&dstinfo, optimize, progressive, arithmetic);
    jpeg_mem_dest(&dstinfo, &_outarr, &_outsize);
    jpeg_write_coefficients(&dstinfo, dst_coef_arrays);
    jcopy_markers_execute(&srcinfo, &dstinfo, copy);
    jtransform_execute_transformation(&srcinfo, &dstinfo, src_coef_arrays, &transformoption);

    jpeg_finish_compress(&dstinfo);
    jpeg_destroy_compress(&dstinfo);
    jpeg_finish_decompress(&srcinfo);
    jpeg_destroy_decompress(&srcinfo);

    *outsize = _outsize;
    if (_outsize > 0 && _outsize <= inputsize)
        memcpy(outbuffer, _outarr, _outsize * sizeof(char));

    free(_outarr);
    return jsrcerr.pub.num_warnings + jdsterr.pub.num_warnings ? FALSE : TRUE;
}

__declspec(dllexport) boolean OptimizeMemoryToFile(
    unsigned char *inputbuffer,
    const long inputsize,
    const char *filename,
    char copyflag,
    char optimize,
    char progressive,
    char grayscale,
    char trim,
    char arithmetic)
{

    struct jpeg_decompress_struct srcinfo;
    struct jpeg_compress_struct dstinfo;
    struct memjpegtran_error_mgr jsrcerr, jdsterr;
    jvirt_barray_ptr * src_coef_arrays;
    jvirt_barray_ptr * dst_coef_arrays;
    JCOPY_OPTION copy = copyflag == FALSE ? JCOPYOPT_NONE : JCOPYOPT_ALL;
    jpeg_transform_info transformoption;
	FILE * fp;
    transformoption.transform = JXFORM_NONE;
    transformoption.perfect = FALSE;
    transformoption.trim = trim;
    transformoption.force_grayscale = grayscale;
    transformoption.crop = FALSE;
    transformoption.crop_width_set = JCROP_UNSET;
    transformoption.crop_height_set = JCROP_UNSET;
    transformoption.crop_xoffset_set = JCROP_UNSET;
    transformoption.crop_yoffset_set = JCROP_UNSET;

	srcinfo.err = jpeg_std_error(&jsrcerr.pub);
	jsrcerr.pub.error_exit = memjpegtran_error_exit;
	jpeg_create_decompress(&srcinfo);
	if (setjmp(jsrcerr.setjmp_buffer))
	{
		jpeg_destroy_compress(&dstinfo);
		jpeg_destroy_decompress(&srcinfo);

		if (fp !=NULL)
			fclose(fp);
		return FALSE;
	}
	dstinfo.err = jpeg_std_error(&jdsterr.pub);
	jdsterr.pub.error_exit = memjpegtran_error_exit;
	jpeg_create_compress(&dstinfo);
	if (setjmp(jdsterr.setjmp_buffer))
	{
		jpeg_destroy_compress(&dstinfo);
		jpeg_destroy_decompress(&srcinfo);

		if (fp != NULL)
			fclose(fp);
		return FALSE;
	}

    prepare_options(&dstinfo, optimize, progressive, arithmetic);
	jsrcerr.pub.trace_level = jdsterr.pub.trace_level;
    srcinfo.mem->max_memory_to_use = dstinfo.mem->max_memory_to_use;

    jpeg_mem_src(&srcinfo, inputbuffer, inputsize);
    jcopy_markers_setup(&srcinfo, copy);

    (void)jpeg_read_header(&srcinfo, TRUE);

    if (!jtransform_request_workspace(&srcinfo, &transformoption))
    {
        return FALSE;
    }

    src_coef_arrays = jpeg_read_coefficients(&srcinfo);
    jpeg_copy_critical_parameters(&srcinfo, &dstinfo);

    dst_coef_arrays = jtransform_adjust_parameters(&srcinfo, &dstinfo,
        src_coef_arrays,
        &transformoption);

   
    if ((fp = fopen(filename, WRITE_BINARY)) == NULL)
    {
        return FALSE;
    }

    prepare_options(&dstinfo, optimize, progressive, arithmetic);
    jpeg_stdio_dest(&dstinfo, fp);
    jpeg_write_coefficients(&dstinfo, dst_coef_arrays);
    jcopy_markers_execute(&srcinfo, &dstinfo, copy);

    jtransform_execute_transformation(&srcinfo, &dstinfo,
        src_coef_arrays,
        &transformoption);

    jpeg_finish_compress(&dstinfo);
    jpeg_destroy_compress(&dstinfo);
    jpeg_finish_decompress(&srcinfo);
    jpeg_destroy_decompress(&srcinfo);

    fclose(fp);
    
    return jsrcerr.pub.num_warnings + jdsterr.pub.num_warnings ? FALSE : TRUE;
}

__declspec(dllexport) boolean OptimizeFileToFile(
    const char *inpfilename,
    const char *outfilename,
    char copyflag,
    char optimize,
    char progressive,
    char grayscale,
    char trim,
    char arithmetic)
{
    FILE * fp;
    struct jpeg_decompress_struct srcinfo;
    struct jpeg_compress_struct dstinfo;
    struct memjpegtran_error_mgr jsrcerr, jdsterr;
    jvirt_barray_ptr * src_coef_arrays;
    jvirt_barray_ptr * dst_coef_arrays;
    JCOPY_OPTION copy = copyflag == FALSE ? JCOPYOPT_NONE : JCOPYOPT_ALL;
    jpeg_transform_info transformoption;
    transformoption.transform = JXFORM_NONE;
    transformoption.perfect = FALSE;
    transformoption.trim = trim;
    transformoption.force_grayscale = grayscale;
    transformoption.crop = FALSE;
    transformoption.crop_width_set = JCROP_UNSET;
    transformoption.crop_height_set = JCROP_UNSET;
    transformoption.crop_xoffset_set = JCROP_UNSET;
    transformoption.crop_yoffset_set = JCROP_UNSET;

	srcinfo.err = jpeg_std_error(&jsrcerr.pub);
	jsrcerr.pub.error_exit = memjpegtran_error_exit;
	jpeg_create_decompress(&srcinfo);
	if (setjmp(jsrcerr.setjmp_buffer))
	{
		jpeg_destroy_compress(&dstinfo);
		jpeg_destroy_decompress(&srcinfo);

		if (fp != NULL)
			fclose(fp);
		return FALSE;
	}
	dstinfo.err = jpeg_std_error(&jdsterr.pub);
	jdsterr.pub.error_exit = memjpegtran_error_exit;
	jpeg_create_compress(&dstinfo);
	if (setjmp(jdsterr.setjmp_buffer))
	{
		jpeg_destroy_compress(&dstinfo);
		jpeg_destroy_decompress(&srcinfo);

		if (fp != NULL)
			fclose(fp);
		return FALSE;
	}

    prepare_options(&dstinfo, optimize, progressive, arithmetic);
    jsrcerr.pub.trace_level = jdsterr.pub.trace_level;
    srcinfo.mem->max_memory_to_use = dstinfo.mem->max_memory_to_use;

    if ((fp = fopen(inpfilename, READ_BINARY)) == NULL)
    {
        return FALSE;
    }

    jpeg_stdio_src(&srcinfo, fp);
    jcopy_markers_setup(&srcinfo, copy);

    (void)jpeg_read_header(&srcinfo, TRUE);

    if (!jtransform_request_workspace(&srcinfo, &transformoption))
    {
        return FALSE;
    }

    src_coef_arrays = jpeg_read_coefficients(&srcinfo);
    jpeg_copy_critical_parameters(&srcinfo, &dstinfo);

    dst_coef_arrays = jtransform_adjust_parameters(&srcinfo, &dstinfo,
        src_coef_arrays,
        &transformoption);

    fclose(fp);
    if ((fp = fopen(outfilename, WRITE_BINARY)) == NULL)
    {
        return FALSE;
    }

    prepare_options(&dstinfo, optimize, progressive, arithmetic);
    jpeg_stdio_dest(&dstinfo, fp);
    jpeg_write_coefficients(&dstinfo, dst_coef_arrays);
    jcopy_markers_execute(&srcinfo, &dstinfo, copy);

    jtransform_execute_transformation(&srcinfo, &dstinfo,
        src_coef_arrays,
        &transformoption);

    jpeg_finish_compress(&dstinfo);
    jpeg_destroy_compress(&dstinfo);
    (void)jpeg_finish_decompress(&srcinfo);
    jpeg_destroy_decompress(&srcinfo);

    fclose(fp);
    
    return jsrcerr.pub.num_warnings + jdsterr.pub.num_warnings ? FALSE : TRUE;
}
