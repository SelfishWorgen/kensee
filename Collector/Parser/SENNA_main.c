#include <stdio.h>
#include <string.h>
#include <ctype.h>

#include "SENNA_utils.h"
#include "SENNA_Hash.h"
#include "SENNA_Tokenizer.h"

#include "SENNA_POS.h"
#include "SENNA_CHK.h"
#include "SENNA_NER.h"
#include "SENNA_VBS.h"
#include "SENNA_PT0.h"
#include "SENNA_SRL.h"
#include "SENNA_PSG.h"
#include "Parser.h"

#define MAX_TARGET_VB_SIZE 256

static    SENNA_Hash *word_hash = NULL;
static    SENNA_Hash *caps_hash = NULL;
static    SENNA_Hash *suff_hash = NULL;
static    SENNA_Hash *gazt_hash = NULL;

static    SENNA_Hash *gazl_hash = NULL;
static    SENNA_Hash *gazm_hash = NULL;
static    SENNA_Hash *gazo_hash = NULL;
static    SENNA_Hash *gazp_hash = NULL;

    /* labels */
static    SENNA_Hash *pos_hash = NULL;
static    SENNA_Hash *chk_hash = NULL;
static    SENNA_Hash *pt0_hash = NULL;
static    SENNA_Hash *ner_hash = NULL;
static    SENNA_Hash *vbs_hash = NULL;
static    SENNA_Hash *srl_hash = NULL;
static    SENNA_Hash *psg_left_hash = NULL;
static    SENNA_Hash *psg_right_hash = NULL;

static    SENNA_POS *pos = NULL;
static    SENNA_CHK *chk = NULL;
static    SENNA_PT0 *pt0 = NULL;
static    SENNA_NER *ner = NULL;
static    SENNA_VBS *vbs = NULL;
static    SENNA_SRL *srl = NULL;
static    SENNA_PSG *psg = NULL;

PARSER_API void __stdcall  InitializeParser(char *dataPath)
{
	   /* inputs */
    word_hash = SENNA_Hash_new(dataPath, "hash/words.lst");
    caps_hash = SENNA_Hash_new(dataPath, "hash/caps.lst");
    suff_hash = SENNA_Hash_new(dataPath, "hash/suffix.lst");
    gazt_hash = SENNA_Hash_new(dataPath, "hash/gazetteer.lst");

    gazl_hash = SENNA_Hash_new_with_admissible_keys(dataPath, "hash/ner.loc.lst", "data/ner.loc.dat");
    gazm_hash = SENNA_Hash_new_with_admissible_keys(dataPath, "hash/ner.msc.lst", "data/ner.msc.dat");
    gazo_hash = SENNA_Hash_new_with_admissible_keys(dataPath, "hash/ner.org.lst", "data/ner.org.dat");
    gazp_hash = SENNA_Hash_new_with_admissible_keys(dataPath, "hash/ner.per.lst", "data/ner.per.dat");

    /* labels */
    pos_hash = SENNA_Hash_new(dataPath, "hash/pos.lst");
    chk_hash = SENNA_Hash_new(dataPath, "hash/chk.lst");
    pt0_hash = SENNA_Hash_new(dataPath, "hash/pt0.lst");
    ner_hash = SENNA_Hash_new(dataPath, "hash/ner.lst");
    vbs_hash = SENNA_Hash_new(dataPath, "hash/vbs.lst");
    srl_hash = SENNA_Hash_new(dataPath, "hash/srl.lst");
    psg_left_hash = SENNA_Hash_new(dataPath, "hash/psg-left.lst");
    psg_right_hash = SENNA_Hash_new(dataPath, "hash/psg-right.lst");

    pos = SENNA_POS_new(dataPath, "data/pos.dat");
    chk = SENNA_CHK_new(dataPath, "data/chk.dat");
    pt0 = SENNA_PT0_new(dataPath, "data/pt0.dat");
    ner = SENNA_NER_new(dataPath, "data/ner.dat");
    vbs = SENNA_VBS_new(dataPath, "data/vbs.dat");
    srl = SENNA_SRL_new(dataPath, "data/srl.dat");
    psg = SENNA_PSG_new(dataPath, "data/psg.dat");
}

PARSER_API void __stdcall FinalyzeParser()
{
	  SENNA_POS_free(pos);
    SENNA_CHK_free(chk);
    SENNA_PT0_free(pt0);
    SENNA_NER_free(ner);
    SENNA_VBS_free(vbs);
    SENNA_SRL_free(srl);
    SENNA_PSG_free(psg);

    SENNA_Hash_free(word_hash);
    SENNA_Hash_free(caps_hash);
    SENNA_Hash_free(suff_hash);
    SENNA_Hash_free(gazt_hash);

    SENNA_Hash_free(gazl_hash);
    SENNA_Hash_free(gazm_hash);
    SENNA_Hash_free(gazo_hash);
    SENNA_Hash_free(gazp_hash);

    SENNA_Hash_free(pos_hash);
    SENNA_Hash_free(chk_hash);
    SENNA_Hash_free(pt0_hash);
    SENNA_Hash_free(ner_hash);
    SENNA_Hash_free(vbs_hash);
    SENNA_Hash_free(srl_hash);
    SENNA_Hash_free(psg_left_hash);
    SENNA_Hash_free(psg_right_hash);
}

PARSER_API void __stdcall ParseText(char *text, char *dataPath, CallBackFn callBackFn)
{
  int i;

  /* options */
  char *opt_path = NULL;

	if (word_hash == NULL)
		InitializeParser(dataPath);

  /* the real thing */
  {
    char *sentence;
    int *chk_labels = NULL;
    int *pt0_labels = NULL;
    int *pos_labels = NULL;
    int *ner_labels = NULL;
    int *vbs_labels = NULL;
    int **srl_labels = NULL;
    int *psg_labels = NULL;
    int n_psg_level = 0;
    int is_psg_one_segment = 0;
    int vbs_hash_novb_idx = 22;
    int n_verbs = 0;
    char *orig_text = text;

    SENNA_Tokenizer *tokenizer = SENNA_Tokenizer_new(word_hash, caps_hash, suff_hash, gazt_hash, gazl_hash, gazm_hash, gazo_hash, gazp_hash);

    while (*text != 0)
    {
			SENNA_Tokens* tokens;
			char *p = strchr(text, '\n');
			sentence = text;
			if (p)
			{
				*p = 0;
				text = p + 1;
			}
			else
			{
				text = text + strlen(text);
			}
      tokens = SENNA_Tokenizer_tokenize(tokenizer, sentence);
    
      if(tokens->n == 0)
			{
//				callBackFn("", 0, 0, "", "", "");
        continue;
			}
      pos_labels = SENNA_POS_forward(pos, tokens->word_idx, tokens->caps_idx, tokens->suff_idx, tokens->n);
      chk_labels = SENNA_CHK_forward(chk, tokens->word_idx, tokens->caps_idx, pos_labels, tokens->n);
      ner_labels = SENNA_NER_forward(ner, tokens->word_idx, tokens->caps_idx, tokens->gazl_idx, tokens->gazm_idx, tokens->gazo_idx, tokens->gazp_idx, tokens->n);

      for(i = 0; i < tokens->n; i++)
      {
				callBackFn(tokens->words[i], sentence - orig_text + tokens->start_offset[i], tokens->end_offset[i], 
					SENNA_Hash_key(pos_hash, pos_labels[i]),
					SENNA_Hash_key(chk_hash, chk_labels[i]),
					SENNA_Hash_key(ner_hash, ner_labels[i]));
      }
			callBackFn("", 0, 0, "", "", "");
    }

    SENNA_Tokenizer_free(tokenizer);
  }
}
