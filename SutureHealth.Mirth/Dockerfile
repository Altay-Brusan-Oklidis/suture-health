FROM nextgenhealthcare/connect:4.2-jdk     

ARG USER_HOME_DIR="/root"
USER root

RUN mkdir -p /usr/share/maven /usr/share/maven/ref 
RUN curl -fsSL -o /tmp/apache-maven.tar.gz https://dlcdn.apache.org/maven/maven-3/3.9.1/binaries/apache-maven-3.9.1-bin.tar.gz 
RUN tar -xzf /tmp/apache-maven.tar.gz -C /usr/share/maven --strip-components=1
RUN rm -f /tmp/apache-maven.tar.gz
RUN ln -s /usr/share/maven/bin/mvn /usr/bin/mvn

ENV MAVEN_HOME /usr/share/maven
ENV MAVEN_CONFIG "$USER_HOME_DIR/.m2"

COPY extra/MirthAWS/pom.xml ./pom.xml
COPY extra/MirthAWS/src ./src
COPY mirth/packages ./custom-lib

RUN mvn clean
RUN mvn package
RUN cp "./target/mirthaws-1.0.jar" "./custom-lib/mirthaws-1.0.jar"

CMD [ "./mcserver" ]
