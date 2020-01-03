# mLauncher

## 윈도우 런처 프로그램

![mLauncher](https://github.com/motolies/mLauncher/blob/master/doc/imange/mainImange.png)

위 화면과 같이 mLauncher를 실행시키면 바둑판과 같은 모양의 버튼들이 생성됩니다.
해당 버튼에 파일 및 폴더를 드래그해서 올려주시면 바로가기 기능을 사용할 수 있습니다.

### 기능
* 버튼 위에 드래그 앤 드롭
* 핫키
  * 글로벌 핫키
    * [Ctrl + Shift + A] mLauncher 프로그램 띄우기(실행중이어야 함)
    * [Ctrl + Shift + Q] mFileSearch 프로그램 실행(mLauncher가 실행중이어야 함) 
  * 로컬 핫키(프로그램에 포커스가 가 있는 상태)
    * [F12] 설정창 표시

## 파일 내용 검색 프로그램

![mFileSearch](https://github.com/motolies/mLauncher/blob/master/doc/imange/mFileSearchMainImage.png)

검색 대상 폴더에서 특정 확장자만 필터링 하여 **파일내용** 검색할 수 있는 프로그램으로 정규식을 지원함
다만 파일을 line 별로 읽어오느라 정규식의 경우 singleline 옵션이 들어가 있음

### 기능
* 폴더 내 파일 확장자 필터링 후 내용검색
* 애초에 검색하지 않을 파일 확장자 지정가능
* 탐색기에서 우클릭 사용으로 바로 호출가능(레지스트리 등록)




