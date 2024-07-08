# GOPool

### 소개
유니티의 IObjectPool을 사용해서 구현한 게임 오브젝트 풀링 시스템

### 설치방법
1. 패키지 관리자의 툴바에서 좌측 상단에 플러스 메뉴를 클릭합니다.
2. 추가 메뉴에서 Add package from git URL을 선택하면 텍스트 상자와 Add 버튼이 나타납니다.
3. https://github.com/DarkNaku/GOPool.git 입력하고 Add를 클릭합니다.

### 사용방법
```csharp
GOPool.RegisterBuiltIn("Prefabs/Capsule", "Prefabs/Cube", "Prefabs/Sphere"); // 리소스 경로에 있는 프리팹 등록

var cube = GOPool.Get(Cube); // 사용 방법 1
var customComponent = GOPool.Get<CustomComponent>(key); // 사용 방법 2

GOPool.Release(cube); // 해제 1
GOPool.Release(customComponent); // 해제 2
GOPool.Release(cube, 1f); // 지연 해제 3
GOPool.Release(customComponent, 2f); // 지연 해제 4
```

### 추가 하려고 계획하고 있는 기능
* 비동기 웜업
* 어드레서블로 부터 추가
