#ifndef MK_XRAY_DEF
	#define MK_XRAY_DEF

	//Dissolve
	#if _MK_DISSOLVE_DEFAULT
		#ifndef _MKXRAY_DISSOLVE
			#define _MKXRAY_DISSOLVE 1
		#endif
	#endif

	//Emission
	#if (_MK_EMISSION_DEFAULT || _MK_EMISSION_MAP)
		#ifndef _MKXRAY_EMISSION
			#define _MKXRAY_EMISSION 1
		#endif
	#endif
	
	//Texcoords
	#if _MKXRAY_DISSOLVE || _MKXRAY_EMISSION || _MK_ALBEDO_MAP
		#ifndef MKXRAY_TC
			#define MKXRAY_TC 1
		#endif
	#endif

#endif